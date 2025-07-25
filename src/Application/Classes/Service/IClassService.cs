using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Service;

public interface IClassService
{
    Task<(ClassUser IsOwner, Class ClassExists)> GetClassOwnerAccess(Guid classId,
        CancellationToken cancellationToken);
    Task<bool> IsStudentInClass(Guid classId);
    Task<bool> IsUserInClass(Guid classId);
    Task<bool> IsLecturerOrOwnerInClass(Guid classId);
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

    public async Task<bool> IsStudentInClass(Guid classId)
    {
        var student = await _context.ClassUsers
            .Where(u => u.UserId == _user.UserId && u.ClassId == classId && ClassShareMode.Student == u.ShareMode)
            .FirstOrDefaultAsync();

        if (student == null)
            return false;

        return true;
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
}
