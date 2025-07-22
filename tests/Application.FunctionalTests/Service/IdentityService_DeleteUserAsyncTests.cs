using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_DeleteUserAsyncTests : IdentityServiceTestBase
{
    [Test]
    public async Task DeleteUserAsync_UserExists_ReturnsResult()
    {
        var userId = Guid.NewGuid();
        var user = new UserAccount { Id = userId };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _service.DeleteUserAsync(userId);

        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task DeleteUserAsync_UserNotFound_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var result = await _service.DeleteUserAsync(userId);

        Assert.That(result.Succeeded, Is.True);
    }
} 
