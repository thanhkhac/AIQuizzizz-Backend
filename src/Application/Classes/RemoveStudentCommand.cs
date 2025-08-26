using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

[Authorize]
public class RemoveStudentCommand : IRequest<Guid>
{
    /// <summary>
    /// Id of the class want to remove student
    /// </summary>
    public required Guid ClassId { get; set; }   
    /// <summary>
    /// Id of the student want to remove from class
    /// </summary>
    public required Guid UserId { get; set; }
}

public class RemoveStudentCommandValidator : AbstractValidator<RemoveStudentCommand>
{
    public RemoveStudentCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không thể trống");
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("StudentId không thể trống");
    }
}

public class RemoveStudentCommandHandler : IRequestHandler<RemoveStudentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public RemoveStudentCommandHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    /// <summary>
    /// The function removes a student from a class and returns the student's user ID
    /// </summary>
    /// <param name="rq">Request contains ClassId and UserId information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<Guid> Handle(RemoveStudentCommand rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND);
        
        var canDelete = await _classService.IsLecturerOrOwnerInClass(rq.ClassId);
        if (!canDelete)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION);
        
        var user = await _context.ClassUsers
            .Include(u => u.User)
            .Where(cu => cu.ClassId == rq.ClassId && cu.ShareMode != ClassShareMode.Owner && cu.Class.IsDeleted == false && cu.UserId== rq.UserId)
            .Select(u => new { User = u.User, ClassUser = u})
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null || user.ClassUser == null)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_USER_IN_CLASS, "User không tồn tại hoặc không trong lớp");

        if (user.ClassUser.ShareMode == ClassShareMode.Teacher)
        {
            await _classService.DeleteQuestionSetByUserId(rq.UserId, rq.ClassId);
        }
    
        _context.ClassUsers.Remove(user.ClassUser);
        await _context.SaveChangesAsync(cancellationToken);
        return user.User.Id;
    }
}
