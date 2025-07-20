using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

public class ClassCodeDto
{
    public required string Code { get; set; }
}

[Authorize]
public class CreateInviteCodeCommand : IRequest<ClassCodeDto>
{
    public required Guid ClassId { get; set; }
    public required double ExpiredTime { get; set; }
}

public class CreateInviteCodeCommandValidator : AbstractValidator<CreateInviteCodeCommand>
{
    public CreateInviteCodeCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
        RuleFor(x => x.ExpiredTime)
            .NotEmpty().WithMessage("Thời gian hết hạn không được trống")
            .GreaterThan(0).WithMessage("Thời gian hết hạn > 0");
    }
}

public class CreateInviteCodeCommandHandler : IRequestHandler<CreateInviteCodeCommand, ClassCodeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public CreateInviteCodeCommandHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<ClassCodeDto> Handle(CreateInviteCodeCommand rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");
        
        await _classService.IsLecturerOrOwnerInClass(rq.ClassId);

        var inviteCode = await _context.ClassInvitations
            .Where(x => x.ClassId == rq.ClassId && x.IsDeleted == false)
            .ToListAsync(cancellationToken);
        
        var classInvitation = new ClassInvitation()
        {
            Id = Guid.NewGuid(),
            ClassId = classById.Id,
            TimeStart = DateTime.UtcNow,
            TimeEnd = DateTime.UtcNow.AddDays(rq.ExpiredTime),
            Code = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..12],
            IsDeleted = false
        };
        
        _context.ClassInvitations.RemoveRange(inviteCode);
        
        _context.ClassInvitations.Add(classInvitation);
        
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return new ClassCodeDto { Code = classInvitation.Code };
    }
}
