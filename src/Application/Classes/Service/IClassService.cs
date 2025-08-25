using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Service;

public interface IClassService
{
    Task<(ClassUser IsOwner, Class ClassExists)> GetClassOwnerAccess(Guid classId,
        CancellationToken cancellationToken);
    Task<bool> IsUserInClass(Guid classId);
    Task<bool> IsLecturerOrOwnerInClass(Guid classId);
    Task DeleteQuestionSetByUserId(Guid userId, Guid classId);
    Task<Guid> MoveOutClass(Guid classId, CancellationToken cancellationToken);
}

public class ClassService : IClassService{
    
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public ClassService(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<(ClassUser IsOwner, Class ClassExists)> GetClassOwnerAccess(Guid classId, CancellationToken cancellationToken)
    {
        var result = await _context.Classes
            .Where(c => c.Id == classId && c.IsDeleted == false)
            .GroupJoin(
                _context.ClassUsers.Where(cu => cu.UserId == _user.UserId &&
                                                cu.ClassId == classId &&
                                                cu.User.IsBanned == false &&
                                                cu.User.IsDeleted == false),
                c => c.Id,
                cu => cu.ClassId,
                (c, cu) => new
                {
                    Class = c,
                    ClassUser = cu.FirstOrDefault(cu => cu.ShareMode == ClassShareMode.Owner)
                })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null || result.Class == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");

        if (result.ClassUser == null)
            throw new ErrorCodeException(ErrorCodes.ONLY_OWNERS_CAN_UPDATE, "Không tìm thấy lecturer hoặc không phải lecturer của lớp");

        return (result.ClassUser, result.Class);
    }
    
    public async Task<bool> IsUserInClass(Guid classId)
    {
        var user = await _context.ClassUsers
            .Where(u => u.UserId == _user.UserId && u.ClassId == classId)
            .FirstOrDefaultAsync();

        if (user == null)
            return false;

        return true;
    }

    public async Task<bool> IsLecturerOrOwnerInClass(Guid classId)
    {
        var user = await _context.ClassUsers
            .Where(u => u.UserId == _user.UserId && u.ClassId == classId &&
                        (ClassShareMode.Owner.Equals(u.ShareMode) || ClassShareMode.Teacher.Equals(u.ShareMode)))
            .FirstOrDefaultAsync();
        if (user == null) return false;

        return true;
    }

    public async Task DeleteQuestionSetByUserId(Guid userId, Guid classId)
    {
        var questionSets = await _context.ClassQuestionSets
            .Where(x => x.ClassId.Equals(classId) && x.CreatedBy.Equals(userId))
            .ToListAsync();
        
        _context.ClassQuestionSets.RemoveRange(questionSets);
        
        await _context.SaveChangesAsync(new CancellationToken());
    }

    public async Task<Guid> MoveOutClass(Guid classId, CancellationToken cancellationToken)
    {
        var classUser = await _context.ClassUsers
            .Where(x => x.ClassId.Equals(classId) && x.UserId.Equals(_user.UserId))
            .FirstOrDefaultAsync(cancellationToken);
        if (classUser == null)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_USER_IN_CLASS);

        if (ClassShareMode.Owner == classUser.ShareMode)
            throw new ErrorCodeException(ErrorCodes.OWNER_CAN_NOT_MOVE_OUT_CLASS);

        if (classUser.ShareMode == ClassShareMode.Teacher)
        {
           await DeleteQuestionSetByUserId(classUser.UserId, classId);
        }
        
        _context.ClassUsers.Remove(classUser);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return classUser.UserId;
    }
}
