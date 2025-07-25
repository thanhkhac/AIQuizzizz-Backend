using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Moq.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_RefreshTokenAsyncTests : IdentityServiceTestBase
{
    private string accesstoken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsImV4cCI6MTc1MzIxNjQyNywiaXNzIjoiaXNzdWVyIiwiYXVkIjoiYXVkIn0.xUbpZGUsT4UaHTS033V7S58Ct4x4VXtgII2KjB0gh4Q";

    [Test]
    public void RefreshTokenAsync_RefreshTokenNotFound_ThrowsInvalidCredentials()
    {
        _dbContextMock.Setup(x => x.Set<RefreshToken>())
            .ReturnsDbSet(Array.Empty<RefreshToken>());
        
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RefreshTokenAsync(accesstoken, "5fbb94b3-6280-45bf-923a-404a18cadd93"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }

    [Test]
    public void RefreshTokenAsync_RefreshTokenExpired_ThrowsInvalidCredentials()
    {
        var expiredToken = new RefreshToken
        {
            Id = ("00000000-0000-0000-0000-000000000000"),
            Token = "5fbb94b3-6280-45bf-923a-404a18cadd93",
            UserAccountId = Guid.NewGuid(),
            ExpireAt = DateTime.UtcNow.AddMinutes(-1)
        };
        _dbContextMock.Setup(x => x.Set<RefreshToken>()).ReturnsDbSet(new[] { expiredToken });

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RefreshTokenAsync(accesstoken, "5fbb94b3-6280-45bf-923a-404a18cadd93"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }

    [Test]
    public void RefreshTokenAsync_InvalidAccessToken_ThrowsInvalidCredentials()
    {
        var token = new RefreshToken
        {
            Id = ("00000000-0000-0000-0000-000000000000"),
            Token = "5fbb94b3-6280-45bf-923a-404a18cadd93",
            UserAccountId = Guid.NewGuid(),
            ExpireAt = DateTime.UtcNow.AddMinutes(10)
        };
        _dbContextMock.Setup(x => x.Set<RefreshToken>()).ReturnsDbSet(new[] { token });

        // access token không decode ra userId hợp lệ
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RefreshTokenAsync("invalidaccesstoken", "5fbb94b3-6280-45bf-923a-404a18cadd93"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }

    [Test]
    public void RefreshTokenAsync_UserNotFound_ThrowsInvalidCredentials()
    {
        var userId = Guid.NewGuid();
        var token = new RefreshToken
        {
            Id = ("00000000-0000-0000-0000-000000000000"),
            Token = "5fbb94b3-6280-45bf-923a-404a18cadd93",
            UserAccountId = userId,
            ExpireAt = DateTime.UtcNow.AddMinutes(10)
        };
        _dbContextMock.Setup(x => x.Set<RefreshToken>()).ReturnsDbSet(new[] { token });

        // Mock giải mã access token trả về userId đúng
        // Nhưng user không tồn tại
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RefreshTokenAsync("accesstoken", "5fbb94b3-6280-45bf-923a-404a18cadd93"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }

    [Test]
    public void RefreshTokenAsync_UserBanned_ThrowsInvalidCredentials()
    {
        var userId = Guid.NewGuid();
        var token = new RefreshToken
        {
            Id = ("00000000-0000-0000-0000-000000000000"),
            Token = "token",
            UserAccountId = userId,
            ExpireAt = DateTime.UtcNow.AddMinutes(10)
        };
        _dbContextMock.Setup(x => x.Set<RefreshToken>())
            .ReturnsDbSet(new[] { token });

        var user = new UserAccount { Id = userId, IsBanned = true };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() =>
            _service.RefreshTokenAsync(accesstoken, token.Token));

        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }
    
    [Test]
    public void RefreshTokenAsync_UserLockedOut_ThrowsInvalidCredentials()
    {
        var userId = Guid.NewGuid();
        var token = new RefreshToken
        {
            Id = ("00000000-0000-0000-0000-000000000000"),
            Token = "token",
            UserAccountId = userId,
            ExpireAt = DateTime.UtcNow.AddMinutes(10)
        };
        _dbContextMock.Setup(x => x.Set<RefreshToken>())
            .ReturnsDbSet(new[] { token });

        var user = new UserAccount { Id = userId };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() =>
            _service.RefreshTokenAsync(accesstoken, token.Token));

        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS));
    }


    [Test]
    public async Task RefreshTokenAsync_Success_ReturnsTokenDto()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var token = new RefreshToken
        {
            Id = ("00000000-0000-0000-0000-000000000000"),
            Token = "5fbb94b3-6280-45bf-923a-404a18cadd93",
            UserAccountId = userId,
            ExpireAt = DateTime.UtcNow.AddMinutes(10)
        };
        _dbContextMock.Setup(x => x.Set<RefreshToken>()).ReturnsDbSet(new[] { token });

        var user = new UserAccount { Id = userId, Email = "user@example.com" };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);


        var result = await _service.RefreshTokenAsync(accesstoken, "5fbb94b3-6280-45bf-923a-404a18cadd93");
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        TestContext.WriteLine(json);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AccessToken);
        Assert.IsNotNull(result.RefreshToken);
        Assert.Greater(result.ExpireMin, 0);
    }
} 
