using CleanArchitectureBase.Application.Classes.Common;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

[Authorize]
public class RemoveStudentCommand : IRequest<Guid>
{
    public required Guid ClassId { get; set; }   
    public required Guid UserId { get; set; }
}

public class RemoveStudentValidator : AbstractValidator<RemoveStudentCommand>
{
    public RemoveStudentValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không thể trống");
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("StudentId không thể trống");
    }
}

public class RemoveStudentHandler : IRequestHandler<RemoveStudentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ClassValidationService _classValidationService;
    
    public RemoveStudentHandler(IApplicationDbContext context, ClassValidationService classValidationService)
    {
        _context = context;
        _classValidationService = classValidationService;
    }
    
    public async Task<Guid> Handle(RemoveStudentCommand rq, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers
            .Where(x => x.Id == rq.UserId && x.IsDeleted == false && x.IsBanned == false)
            .Join(
                _context.ClassUsers.Where(c => c.ClassId == rq.ClassId && c.ShareMode != ClassShareMode.Owner && c.Class.IsDeleted == false),
                u => u.Id,
                cu => cu.UserId,
                (u, cu) => new { User = u, ClassUser = cu }
            )
            .FirstOrDefaultAsync(cancellationToken);
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_STUDENT_IN_CLASS, "Student không tồn tại hoặc không trong lớp");
        
        var (isOwner, classExists) = await _classValidationService.ValidateClassAccessAsync(rq.ClassId, cancellationToken);

        _context.ClassUsers.Remove(user.ClassUser);
        await _context.SaveChangesAsync(cancellationToken);
        return user.User.Id;
    }
}
