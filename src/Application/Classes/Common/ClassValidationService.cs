using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Common;

public class ClassValidationService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public ClassValidationService(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<(ClassUser IsOwner, Class ClassExists)> ValidateClassAccessAsync(Guid classId, CancellationToken cancellationToken)
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

}
