using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

[Authorize]
public class JoinClassByCodeCommand : IRequest<Unit>
{
    public required string Code { get; set; }
}

public class JoinClassByCodeValidator : AbstractValidator<JoinClassByCodeCommand>
{
    public JoinClassByCodeValidator()
    {
        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Mã code không được để trống");
    }
}

public class JoinClassByCodeCommandHandler : IRequestHandler<JoinClassByCodeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public JoinClassByCodeCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    /// <summary>
    /// The function allows a user to join a class using an invitation code and registers their participation
    /// </summary>
    /// <param name="rq">Request contains the invitation Code</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<Unit> Handle(JoinClassByCodeCommand rq, CancellationToken cancellationToken)
    {
        var classInvitation = await _context.ClassInvitations
            .Where(x => x.Code == rq.Code &&
                        x.TimeStart <= DateTime.UtcNow && x.TimeEnd >= DateTime.UtcNow &&
                        x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (classInvitation == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_CODE_NOT_FOUND, "Mã code không tồn tại hoặc đã hết hạn");
        
        var classByCode = await _context.Classes
            .Where(x => x.Id == classInvitation.ClassId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (classByCode == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Lớp học không tồn tại");

        var userInClass = await _context.ClassUsers
            .Where(x => x.ClassId == classByCode.Id && x.UserId == _user.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (userInClass != null)
            throw new ErrorCodeException(ErrorCodes.STUDENT_ALREADY_EXISTS_IN_CLASS, "Học sinh đã ở trong lớp");

        var classUser = new ClassUser
        {
            UserId = _user.UserId!.Value,
            ClassId = classByCode.Id,
            ShareMode = ClassShareMode.Student,
            Class = classByCode
        };

        var classInvitationUser = new ClassInvitationUser
        {
            Id = Guid.NewGuid(),
            UserId = _user.UserId.Value,
            ClassInvitationId = classInvitation.Id,
            TimeJoin = DateTime.UtcNow,
            ClassInvitation = classInvitation
        };
        
        _context.ClassUsers.Add(classUser);
        _context.ClassInvitationUsers.Add(classInvitationUser);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return Unit.Value;
    }
}
