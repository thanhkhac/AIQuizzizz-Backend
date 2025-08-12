using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_TrySetPasswordAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void TrySetPasswordAsync_UserNotFound_ThrowsAccountNotFound()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TrySetPasswordAsync(userId, "newpass"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public async Task TrySetPasswordAsync_Success()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var user = new UserAccount
        {
            Id = userId,
            Email = "user@example.com"
        };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");
        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, "token", "newpass")).ReturnsAsync(IdentityResult.Success);

        // Không ném exception là thành công
        await _service.TrySetPasswordAsync(userId, "newpass");
    }

    [Test]
    public void TrySetPasswordAsync_ResetPasswordFailed_ThrowsInternalError()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var user = new UserAccount
        {
            Id = userId,
            Email = "user2@example.com"
        };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");
        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, "token", "newpass")).ReturnsAsync(IdentityResult.Failed());

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TrySetPasswordAsync(userId, "newpass"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.COMMON_SERVER_INTERNAL_ERROR));
    }

    [Test]
    public void TrySetPasswordAsync_UserAlreadyHasPassword_ThrowsUserAlreadyHasPassword()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var user = new UserAccount
        {
            Id = userId,
            Email = "user3@example.com"
        };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.HasPasswordAsync(user)).ReturnsAsync(true);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TrySetPasswordAsync(userId, "newpass"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.IDENTITY_USER_ALREADY_HAS_PASSWORD));
    }
}
