using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.TestTemplates.Service;

public interface ITestTemplateService
{
    Task CanViewTesTemplate (Guid testTemplateId);
}

public class TestTemplateService : ITestTemplateService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public TestTemplateService(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task CanViewTesTemplate(Guid testTemplateId)
    {
        var accessToView = await _context.TestTemplateUsers
            .Where(t => t.UserId == _user.UserId && t.TestTemplateId == testTemplateId)
            .FirstOrDefaultAsync();
        
        var accessToViewInFolder = await _context.FolderTestTemplates
            .Include(x => x.Folder!)
            .ThenInclude(x => x.FolderUsers)
            .Where(x => x.TestTemplateId.Equals(testTemplateId) && x.Folder!.FolderUsers.Any(y => y.UserId == _user.UserId))
            .FirstOrDefaultAsync();
        
        if (accessToView == null && accessToViewInFolder == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE, "User không có quyền xem test template này");
    }
}
