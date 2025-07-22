using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

[TestFixture]
public class IdentityService_ChangeRoleAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void ChangeRoleAsync_UserNotFound_ThrowsAccountNotFound()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ChangeRoleAsync(userId, "Admin"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public async Task ChangeRoleAsync_UserAlreadyInRole_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var user = new UserAccount { Id = userId };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new[] { "Admin" });

        var result = await _service.ChangeRoleAsync(userId, "Admin");

        Assert.That(result, Is.EqualTo(userId));
    }
} 
