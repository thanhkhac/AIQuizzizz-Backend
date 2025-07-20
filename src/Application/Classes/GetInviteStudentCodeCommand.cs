using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes;

[Authorize]
public class GetInviteStudentCodeCommand : IRequest<string?>
{
    public required Guid ClassId { get; set; }   
}

public class GetInviteStudentCodeCommandValidator : AbstractValidator<GetInviteStudentCodeCommand>
{
    public GetInviteStudentCodeCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
    }
}

public class GetInviteStudentCodeCommandHandler : IRequestHandler<GetInviteStudentCodeCommand, string?>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService; 
    
    public GetInviteStudentCodeCommandHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<string?> Handle(GetInviteStudentCodeCommand rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");
        
        await _classService.IsLecturerOrOwnerInClass(rq.ClassId);
        
        var code = await _context.ClassInvitations
            .Where(x => x.ClassId == rq.ClassId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        
        return code != null ? code.Code : null;
    }
}
