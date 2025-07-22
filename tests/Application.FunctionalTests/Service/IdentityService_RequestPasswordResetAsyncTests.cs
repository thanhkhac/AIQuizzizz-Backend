using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_RequestPasswordResetAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void RequestPasswordResetAsync_UserNotFound_ThrowsAccountNotFound()
    {
        var dto = new ForgotPasswordDto { Email = "notfound@example.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RequestPasswordResetAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public void RequestPasswordResetAsync_TooManyRequests_ThrowsPasswordResetRequestTooMany()
    {
        var dto = new ForgotPasswordDto { Email = "locked@example.com" };
        var user = new UserAccount
        {
            Email = dto.Email,
            PasswordResetRequestAttempts = 10,
            PasswordResetRequestLockoutEnd = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RequestPasswordResetAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.PASSWORD_RESET_REQUEST_TOO_MANY));
    }

    [Test]
    public async Task RequestPasswordResetAsync_Success()
    {
        var dto = new ForgotPasswordDto { Email = "ok@example.com" };
        var user = new UserAccount { Email = dto.Email };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _emailServiceMock.Setup(x => x.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        await _service.RequestPasswordResetAsync(dto);

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
} 
