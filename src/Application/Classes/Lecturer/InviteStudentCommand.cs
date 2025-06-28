using CleanArchitectureBase.Application.Classes.Common;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class ClassCodeDto
{
    public required string Code { get; set; }
}
public class InviteStudentCommand : IRequest<ClassCodeDto>
{
    public required Guid ClassId { get; set; }
    public required double ExpiredTime { get; set; }
}

public class InviteStudentValidator : AbstractValidator<InviteStudentCommand>
{
    public InviteStudentValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
        RuleFor(x => x.ExpiredTime)
            .NotEmpty().WithMessage("Thời gian hết hạn không được trống")
            .GreaterThan(0).WithMessage("Thời gian hết hạn > 0");
    }
}

public class InviteStudentCommandHandler : IRequestHandler<InviteStudentCommand, ClassCodeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ClassValidationService _classValidationService;
    
    public InviteStudentCommandHandler(IApplicationDbContext context, ClassValidationService classValidationService)
    {
        _context = context;
        _classValidationService = classValidationService;
    }
    
    public async Task<ClassCodeDto> Handle(InviteStudentCommand rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classValidationService.ValidateClassAccessAsync(rq.ClassId, cancellationToken);
        
        var classInvitation = new ClassInvitation()
        {
            Id = Guid.NewGuid(),
            ClassId = classExists.Id,
            TimeStart = DateTime.UtcNow,
            TimeEnd = DateTime.UtcNow.AddDays(rq.ExpiredTime),
            Code = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..12],
            IsDeleted = false
        };
        
        _context.ClassInvitations.Add(classInvitation);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return new ClassCodeDto { Code = classInvitation.Code };
    }
}
