using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_RevokeRefreshTokenAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void RevokeRefreshTokenAsync_TokenNotFound_ThrowsError()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        _dbContextMock.Setup(x => x.Set<RefreshToken>()).ReturnsDbSet(Array.Empty<RefreshToken>());

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RevokeRefreshTokenAsync("notfoundtoken", userId));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.REFRESHTOKEN_NOTFOUND));
    }

    [Test]
    public async Task RevokeRefreshTokenAsync_TokenExists_DoesNotThrow()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var token = new RefreshToken
        {
            Id = "tokenid",
            Token = "tokentodelete",
            UserAccountId = userId,
            ExpireAt = DateTime.UtcNow.AddMinutes(10)
        };
        var tokens = new List<RefreshToken>
        {
            token
        };
        _dbContextMock.Setup(x => x.Set<RefreshToken>()).ReturnsDbSet(tokens);
        _dbContextMock.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.RevokeRefreshTokenAsync("tokentodelete", userId);
    }

    [Test]
    public async Task RevokeRefreshTokenAsync_ExpiredToken_DoesNotThrow()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var expiredToken = new RefreshToken
        {
            Id = "tokenid2",
            Token = "expiredtoken",
            UserAccountId = userId,
            ExpireAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        };
        var tokens = new List<RefreshToken>
        {
            expiredToken
        };
        _dbContextMock.Setup(x => x.Set<RefreshToken>()).ReturnsDbSet(tokens);
        _dbContextMock.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.RevokeRefreshTokenAsync("expiredtoken", userId);
    }

    [Test]
    public void RevokeRefreshTokenAsync_TokenBelongsToAnotherUser_ThrowsError()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var anotherUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var token = new RefreshToken
        {
            Id = "tokenid",
            Token = "tokentodelete",
            UserAccountId = anotherUserId, // Token thuộc về user khác
            ExpireAt = DateTime.UtcNow.AddMinutes(10)
        };
        var tokens = new List<RefreshToken>
        {
            token
        };
        _dbContextMock.Setup(x => x.Set<RefreshToken>()).ReturnsDbSet(tokens);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.RevokeRefreshTokenAsync("tokentodelete", userId));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.REFRESHTOKEN_NOTFOUND));
    }
}
