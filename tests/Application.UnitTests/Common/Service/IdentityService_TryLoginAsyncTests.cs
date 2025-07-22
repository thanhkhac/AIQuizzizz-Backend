using Microsoft.AspNetCore.Identity;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

[TestFixture]
public class IdentityService_TryLoginAsyncTests : IdentityServiceTestBase
{

    [Test]
    public void TryLoginAsync_UserNotFound_ThrowsInvalidCredentials()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("notfound@example.com"))
            .ReturnsAsync((UserAccount)null);
    
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TryLoginAsync("notfound@example.com", "pass"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }

    [Test]
    public void TryLoginAsync_UserBanned_ThrowsAccountBanned()
    {
        var user = new UserAccount { IsBanned = true, EmailConfirmed = true };
        _userManagerMock.Setup(x => x.FindByEmailAsync("banned@example.com"))
            .ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TryLoginAsync("banned@example.com", "pass"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_BANNED));
    }

    [Test]
    public void TryLoginAsync_EmailNotConfirmed_ThrowsEmailNotVerified()
    {
        var user = new UserAccount { IsBanned = false, EmailConfirmed = false };
        _userManagerMock.Setup(x => x.FindByEmailAsync("notverified@example.com"))
            .ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TryLoginAsync("notverified@example.com", "pass"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_EMAIL_NOT_VERIFIED));
    }

    [Test]
    public void TryLoginAsync_LockedOut_ThrowsAccountLockedOut()
    {
        var user = new UserAccount { IsBanned = false, EmailConfirmed = true };
        _userManagerMock.Setup(x => x.FindByEmailAsync("locked@example.com"))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, "pass", true))
            .ReturnsAsync(SignInResult.LockedOut);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TryLoginAsync("locked@example.com", "pass"));
            Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_LOCKED_OUT));
    }

    [Test]
    public void TryLoginAsync_WrongPassword_ThrowsInvalidCredentials()
    {
        var user = new UserAccount { IsBanned = false, EmailConfirmed = true };
        _userManagerMock.Setup(x => x.FindByEmailAsync("wrongpass@example.com"))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, "wrong", true))
            .ReturnsAsync(SignInResult.Failed);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TryLoginAsync("wrongpass@example.com", "wrong"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }
}
