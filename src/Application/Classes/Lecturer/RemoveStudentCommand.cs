using CleanArchitectureBase.Application.Classes.Service;
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
    private readonly IClassService _classService;
    
    public RemoveStudentHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<Guid> Handle(RemoveStudentCommand rq, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers
            .Include(u => u.ClassUsers)
            .Where(u => u.ClassUsers.Any(cu => cu.ClassId == rq.ClassId && cu.ShareMode != ClassShareMode.Owner && cu.Class.IsDeleted == false))
            .Select(u => new { User = u, ClassUser = u.ClassUsers.FirstOrDefault(cu => cu.ClassId == rq.ClassId) })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null || user.ClassUser == null)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_USER_IN_CLASS, "User không tồn tại hoặc không trong lớp");
        
        var (isOwner, classExists) = await _classService.GetClassOwnerAccess(rq.ClassId, cancellationToken);
    
        _context.ClassUsers.Remove(user.ClassUser);
        await _context.SaveChangesAsync(cancellationToken);
        return user.User.Id;
    }
}
