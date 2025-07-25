

using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_TryGoogleLoginAsyncTests : IdentityServiceTestBase
{
    [Test]
    public async Task TryGoogleLoginAsync_Success_ReturnsTokenDtoJson()
    {
        // Mock dữ liệu trả về từ GoogleAuthService
        var googleUser = new CleanArchitectureBase.Application.Users.Common.GoogleUserDto
        {
            Id = "google-id-123",
            Email = "googleuser@example.com",
            Name = "Google User",
            EmailVerified = true
        };
        _googleAuthServiceMock.Setup(x => x.ExchangeCodeForUserInfoAsync("authcode", "redirecturi"))
            .ReturnsAsync(googleUser);

        // Mock user đã tồn tại trong hệ thống
        var user = new UserAccount { IsBanned = false, EmailConfirmed = true, Email = googleUser.Email };
        _userManagerMock.Setup(x => x.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _service.TryGoogleLoginAsync("authcode", "redirecturi");

        var json = System.Text.Json.JsonSerializer.Serialize(result);
        TestContext.WriteLine(json);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AccessToken);
        Assert.IsNotNull(result.RefreshToken);
        Assert.Greater(result.ExpireMin, 0);
        Assert.That(json, Does.Contain("AccessToken"));
        Assert.That(json, Does.Contain("RefreshToken"));
        Assert.That(json, Does.Contain("ExpireMin"));
    }

    [Test]
    public void TryGoogleLoginAsync_InvalidAuthorizationCode_ThrowsInvalidCredentials()
    {
        _googleAuthServiceMock.Setup(x => x.ExchangeCodeForUserInfoAsync("badcode", "redirecturi"))
            .ThrowsAsync(new Exception("Google error"));

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TryGoogleLoginAsync("badcode", "redirecturi"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }

    [Test]
    public async Task TryGoogleLoginAsync_UserNotExist_CreatesNewUserAndReturnsToken()
    {
        var googleUser = new CleanArchitectureBase.Application.Users.Common.GoogleUserDto
        {
            Id = "google-id-456",
            Email = "newuser@example.com",
            Name = "New User",
            EmailVerified = true
        };
        _googleAuthServiceMock.Setup(x => x.ExchangeCodeForUserInfoAsync("authcode2", "redirecturi2"))
            .ReturnsAsync(googleUser);
        _userManagerMock.Setup(x => x.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync((UserAccount)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserAccount>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.TryGoogleLoginAsync("authcode2", "redirecturi2");
        Assert.IsNotNull(result);
        Assert.IsFalse(result.HasPassword);
        Assert.IsNotNull(result.AccessToken);
        Assert.IsNotNull(result.RefreshToken);
    }

    [Test]
    public void TryGoogleLoginAsync_UserDeleted_ThrowsInvalidCredentials()
    {
        var googleUser = new CleanArchitectureBase.Application.Users.Common.GoogleUserDto
        {
            Id = "google-id-789",
            Email = "deleteduser@example.com",
            Name = "Deleted User",
            EmailVerified = true
        };
        _googleAuthServiceMock.Setup(x => x.ExchangeCodeForUserInfoAsync("authcode3", "redirecturi3"))
            .ReturnsAsync(googleUser);
        var user = new UserAccount { IsDeleted = true, Email = googleUser.Email };
        _userManagerMock.Setup(x => x.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TryGoogleLoginAsync("authcode3", "redirecturi3"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }

    [Test]
    public void TryGoogleLoginAsync_UserBanned_ThrowsAccountBanned()
    {
        var googleUser = new CleanArchitectureBase.Application.Users.Common.GoogleUserDto
        {
            Id = "google-id-101",
            Email = "banneduser@example.com",
            Name = "Banned User",
            EmailVerified = true
        };
        _googleAuthServiceMock.Setup(x => x.ExchangeCodeForUserInfoAsync("authcode4", "redirecturi4"))
            .ReturnsAsync(googleUser);
        var user = new UserAccount { IsBanned = true, Email = googleUser.Email };
        _userManagerMock.Setup(x => x.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TryGoogleLoginAsync("authcode4", "redirecturi4"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_BANNED));
    }

    [Test]
    public async Task TryGoogleLoginAsync_UserNotConfirmedEmail_AutoConfirmAndReturnToken()
    {
        var googleUser = new CleanArchitectureBase.Application.Users.Common.GoogleUserDto
        {
            Id = "google-id-202",
            Email = "notconfirmed@example.com",
            Name = "Not Confirmed",
            EmailVerified = true
        };
        _googleAuthServiceMock.Setup(x => x.ExchangeCodeForUserInfoAsync("authcode5", "redirecturi5"))
            .ReturnsAsync(googleUser);
        var user = new UserAccount { IsBanned = false, EmailConfirmed = false, Email = googleUser.Email };
        _userManagerMock.Setup(x => x.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _service.TryGoogleLoginAsync("authcode5", "redirecturi5");
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AccessToken);
        Assert.IsNotNull(result.RefreshToken);
    }

    [Test]
    public void TryGoogleLoginAsync_CreateUserFailed_ThrowsInternalError()
    {
        var googleUser = new CleanArchitectureBase.Application.Users.Common.GoogleUserDto
        {
            Id = "google-id-303",
            Email = "failcreate@example.com",
            Name = "Fail Create",
            EmailVerified = true
        };
        _googleAuthServiceMock.Setup(x => x.ExchangeCodeForUserInfoAsync("authcode6", "redirecturi6"))
            .ReturnsAsync(googleUser);
        _userManagerMock.Setup(x => x.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync((UserAccount)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserAccount>()))
            .ReturnsAsync(IdentityResult.Failed());

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.TryGoogleLoginAsync("authcode6", "redirecturi6"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.COMMON_SERVER_INTERNAL_ERROR));
    }
}
