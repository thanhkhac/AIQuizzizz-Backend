using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_RequestEmailVerificationAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void RequestEmailVerificationAsync_UserNotFound_ThrowsAccountNotFound()
    {
        var email = "notfound@example.com";
        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RequestEmailVerificationAsync(email));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public void RequestEmailVerificationAsync_TooManyRequests_ThrowsEmailVerificationRequestTooMany()
    {
        var email = "locked@example.com";
        var user = new UserAccount
        {
            Email = email,
            EmailVerificationRequestAttempts = 10,
            EmailVerificationRequestLockoutEnd = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RequestEmailVerificationAsync(email));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.EMAIL_VERIFICATION_REQUEST_TOO_MANY));
    }

    [Test]
    public async Task RequestEmailVerificationAsync_Success()
    {
        var email = "ok@example.com";
        var user = new UserAccount { Email = email };
        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _emailServiceMock.Setup(x => x.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        await _service.RequestEmailVerificationAsync(email);

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
    
   
    [Test]
    public async Task RequestEmailVerificationAsync_AttemptsExceededButLockoutEnded_Success()
    {
        var email = "retry@example.com";
        var user = new UserAccount
        {
            Email = email,
            EmailVerificationRequestAttempts = 5, // >= 5
            EmailVerificationRequestLockoutEnd = DateTimeOffset.UtcNow.AddMinutes(-1) //lockout đã kết thúc
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _emailServiceMock.Setup(x => x.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        await _service.RequestEmailVerificationAsync(email);

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    
} 
