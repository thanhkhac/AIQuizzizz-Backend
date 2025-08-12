using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_VerifyEmailAsyncTests : IdentityServiceTestBase
{

    [Test]
    public void VerifyEmailAsync_UserNotFound_ThrowsAccountNotFound()
    {
        var dto = new EmailVerificationConfirmDto
        {
            Email = "notfound@example.com",
            VerificationCode = "123456"
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.VerifyEmailAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public void VerifyEmailAsync_WrongCode_ThrowsInvalidVerificationCode()
    {
        var dto = new EmailVerificationConfirmDto
        {
            Email = "user@example.com",
            VerificationCode = "wrong"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            EmailVerificationCode = "12345",
            EmailVerificationCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.VerifyEmailAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_VERIFICATION_CODE));
    }

    [Test]
    public async Task VerifyEmailAsync_Success()
    {
        var dto = new EmailVerificationConfirmDto
        {
            Email = "user@example.com",
            VerificationCode = "123456"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            EmailVerificationCode = "123456",
            EmailVerificationCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _service.VerifyEmailAsync(dto);

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.AtLeastOnce);
        Assert.That(user.EmailConfirmed, Is.True);
    }

    [Test]
    public void VerifyEmailAsync_CodeExpired_ThrowsInvalidVerificationCode()
    {
        var dto = new EmailVerificationConfirmDto
        {
            Email = "user@example.com",
            VerificationCode = "123456"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            EmailVerificationCode = "123456",
            EmailVerificationCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(-1)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.VerifyEmailAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_VERIFICATION_CODE));
    }


    [Test]
    public void VerifyEmailAsync_TooManyFailedAttempts_ThrowsCodeFailedTooMany()
    {
        var dto = new EmailVerificationConfirmDto
        {
            Email = "user@example.com",
            VerificationCode = "123456"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            FailedEmailVerificationAttempts = 5,
            EmailVerificationLockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.VerifyEmailAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.EMAIL_VERIFICATION_CODE_FAILED_TOO_MANY));
    }


    [Test]
    public async Task VerifyEmailAsync_TooManyAttempts_LockoutExpired_AllowVerify()
    {
        var dto = new EmailVerificationConfirmDto
        {
            Email = "user@example.com",
            VerificationCode = "123456"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            EmailVerificationCode = "123456",
            EmailVerificationCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5),
            FailedEmailVerificationAttempts = 5,
            EmailVerificationLockoutEnd = DateTimeOffset.UtcNow.AddMinutes(-1)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _service.VerifyEmailAsync(dto);

        Assert.That(user.EmailConfirmed, Is.True);
        Assert.That(user.FailedEmailVerificationAttempts, Is.EqualTo(0));
        Assert.That(user.EmailVerificationLockoutEnd, Is.Null);
    }


}
