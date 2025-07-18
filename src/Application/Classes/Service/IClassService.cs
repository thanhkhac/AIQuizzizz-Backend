using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Service;

public interface IClassService
{
    Task<(ClassUser IsOwner, Class ClassExists)> GetClassOwnerAccess(Guid classId,
        CancellationToken cancellationToken);
    Task IsStudentInClass(Guid classId);
    Task IsUserInClass(Guid classId);
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

    public async Task IsStudentInClass(Guid classId)
    {
        var student = await _context.ClassUsers
            .Where(u => u.UserId == _user.UserId && u.ClassId == classId && ClassShareMode.Student == u.ShareMode)
            .FirstOrDefaultAsync();

        if (student == null)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_STUDENT_IN_CLASS, "Chỉ student trong lớp mới có quyền");
    }

    public async Task IsUserInClass(Guid classId)
    {
        var user = await _context.ClassUsers
            .Where(u => u.UserId == _user.UserId && u.ClassId == classId)
            .FirstOrDefaultAsync();

        if (user == null)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_STUDENT_IN_CLASS, "Student không có trong lớp");
    }
}
