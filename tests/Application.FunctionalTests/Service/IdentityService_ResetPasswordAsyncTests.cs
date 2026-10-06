using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_ResetPasswordAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void ResetPasswordAsync_UserNotFound_ThrowsInvalidResetCode()
    {
        var dto = new ResetPasswordDto
        {
            Email = "notfound@example.com",
            ResetCode = "123456",
            NewPassword = "newpass"
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ResetPasswordAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_RESET_CODE));
    }

    [Test]
    public void ResetPasswordAsync_WrongCode_ThrowsInvalidResetCode()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@example.com",
            ResetCode = "wrong",
            NewPassword = "newpass"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            PasswordResetCode = "right",
            PasswordResetCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ResetPasswordAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_RESET_CODE));
    }

    [Test]
    public async Task ResetPasswordAsync_Success()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@example.com",
            ResetCode = "123456",
            NewPassword = "newpass"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            PasswordResetCode = "123456",
            PasswordResetCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");
        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, "token", dto.NewPassword)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _service.ResetPasswordAsync(dto);

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.AtLeastOnce);
    }

    [Test]
    public void ResetPasswordAsync_UserLockedOut_ThrowsTooManyAttempts()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@example.com",
            ResetCode = "123456",
            NewPassword = "newpass"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            PasswordResetCode = "123456",
            PasswordResetCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5),
            PasswordResetLockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ResetPasswordAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.PASSWORD_RESET_CODE_FAILED_TOO_MANY));
    }


    [Test]
    public void ResetPasswordAsync_LockoutExpired_ResetFailCount()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@example.com",
            ResetCode = "wrong",
            NewPassword = "newpass"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            PasswordResetCode = "123456",
            PasswordResetCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5),
            PasswordResetLockoutEnd = DateTimeOffset.UtcNow.AddMinutes(-1),
            FailedPasswordResetAttempts = 3
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ResetPasswordAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_RESET_CODE));
        Assert.That(user.FailedPasswordResetAttempts, Is.EqualTo(1));
    }


    [Test]
    public void ResetPasswordAsync_CodeExpired_ThrowsInvalidResetCode()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@example.com",
            ResetCode = "123456",
            NewPassword = "newpass"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            PasswordResetCode = "123456",
            PasswordResetCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(-1),
            FailedPasswordResetAttempts = 0
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ResetPasswordAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_RESET_CODE));
        Assert.That(user.FailedPasswordResetAttempts, Is.EqualTo(1));
    }


}
