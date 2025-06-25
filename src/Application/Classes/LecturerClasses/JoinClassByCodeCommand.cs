using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.LecturerClasses;

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
    
    public async Task<Unit> Handle(JoinClassByCodeCommand rq, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers.Where(x => x.Id == _user.UserId).FirstOrDefaultAsync();
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, $"User with id {_user.UserId} not found");
        
        var classInvitation = await _context.ClassInvitations
            .Where(x => x.Code == rq.Code && x.TimeStart <= DateTime.UtcNow && x.TimeEnd >= DateTime.UtcNow)
            .FirstOrDefaultAsync();
        if (classInvitation == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_CODE_NOT_FOUND, "Mã code không tồn tại hoặc đã hết hạn");
        
        var classByCode = await _context.Classes.Where(x => x.Id == classInvitation.ClassId).FirstOrDefaultAsync();
        if (classByCode == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOT_FOUND, "Lớp học không tồn tại");

        var userInClass = await _context.ClassUsers.Where(x => x.ClassId == classByCode.Id && x.UserId == user.Id).FirstOrDefaultAsync();
        if (userInClass != null)
            throw new ErrorCodeException(ErrorCodes.STUDENT_ALREADY_EXISTS, "Học sinh đã ở trong lớp");

        var classUser = new ClassUser
        {
            UserId = user.Id,
            ClassId = classByCode.Id,
            ShareMode = ClassShareMode.Student,
            Class = classByCode
        };

        var classInvitationUser = new ClassInvitationUser
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ClassInvitationId = classInvitation.Id,
            TimeJoin = DateTime.UtcNow,
            User = user,
            ClassInvitation = classInvitation
        };
        
        _context.ClassUsers.Add(classUser);
        _context.ClassInvitationUsers.Add(classInvitationUser);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return Unit.Value;
    }
}
