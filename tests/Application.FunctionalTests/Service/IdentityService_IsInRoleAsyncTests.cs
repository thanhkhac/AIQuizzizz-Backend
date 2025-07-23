using CleanArchitectureBase.Infrastructure.Identity;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_IsInRoleAsyncTests : IdentityServiceTestBase
{

    [Test]
    public async Task IsInRoleAsync_UserInRole_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var user = new UserAccount { Id = userId };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

        var result = await _service.IsInRoleAsync(userId, "Admin");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsInRoleAsync_UserNotFound_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var result = await _service.IsInRoleAsync(userId, "Admin");

        Assert.That(result, Is.False);
    }
} 
