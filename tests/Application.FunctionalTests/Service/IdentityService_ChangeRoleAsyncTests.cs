using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_ChangeRoleAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void ChangeRoleAsync_UserNotFound_ThrowsAccountNotFound()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ChangeRoleAsync(userId, "Admin"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public void ChangeRoleAsync_RoleNotExists_ThrowsRoleNotFound()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var user = new UserAccount
        {
            Id = userId
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("Admin"))
            .ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ChangeRoleAsync(userId, "Admin"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ROLE_NOTFOUND));
    }


    [Test]
    public async Task ChangeRoleAsync_UserHasDifferentRole_ChangesRoleSuccessfully()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var user = new UserAccount
        {
            Id = userId
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("Admin"))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new[]
            {
                "User"
            });

        _userManagerMock.Setup(x => x.RemoveFromRoleAsync(user, "User"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.ChangeRoleAsync(userId, "Admin");

        Assert.That(result, Is.EqualTo(userId));

        _userManagerMock.Verify(x => x.RemoveFromRoleAsync(user, "User"), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
        _userManagerMock.Verify(x => x.UpdateSecurityStampAsync(user), Times.Once);
    }

    [Test]
    public async Task ChangeRoleAsync_UserHasNoRole_AssignsNewRole()
    {
        // Arrange
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var user = new UserAccount
        {
            Id = userId
        };
        var newRole = "Admin";

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(Array.Empty<string>());

        _roleManagerMock.Setup(x => x.RoleExistsAsync(newRole))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.AddToRoleAsync(user, newRole))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.ChangeRoleAsync(userId, newRole);

        // Assert
        Assert.That(result, Is.EqualTo(userId));
        _userManagerMock.Verify(x => x.RemoveFromRoleAsync(It.IsAny<UserAccount>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, newRole), Times.Once);
        _userManagerMock.Verify(x => x.UpdateSecurityStampAsync(user), Times.Once);
    }


}
