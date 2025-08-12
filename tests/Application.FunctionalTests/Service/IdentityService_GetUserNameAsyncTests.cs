using CleanArchitectureBase.Infrastructure.Identity;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_GetUserNameAsyncTests : IdentityServiceTestBase
{

    [Test]
    public async Task GetUserNameAsync_UserExists_ReturnsUserName()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var user = new UserAccount
        {
            Id = userId,
            UserName = "testuser"
        };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        var result = await _service.GetUserNameAsync(userId);

        Assert.That(result, Is.EqualTo("testuser"));
    }


    [Test]
    public async Task GetUserNameAsync_UserNotFound_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var result = await _service.GetUserNameAsync(userId);

        Assert.That(result, Is.Null);
    }
}
