using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Application.Common.Exceptions;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

[TestFixture]
public class IdentityService_CreateUserAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void CreateUserAsync_EmailBanned_ThrowsAccountEmailBanned()
    {
        var user = new UserAccount { IsBanned = true, Email = "banned@example.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("banned@example.com")).ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.CreateUserAsync("banned@example.com", "password"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_EMAIL_BANNED));
    }

    [Test]
    public void CreateUserAsync_DuplicateEmail_ThrowsDuplicateEmail()
    {
        var user = new UserAccount { IsBanned = false, Email = "duplicate@example.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("duplicate@example.com")).ReturnsAsync(user);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.CreateUserAsync("duplicate@example.com", "password"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.IDENTITY_DUPLICATE_EMAIL));
    }

    [Test]
    public async Task CreateUserAsync_Success_ReturnsResultAndUserId()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com")).ReturnsAsync((UserAccount)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserAccount>(), "password")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserAccount>())).ReturnsAsync(IdentityResult.Success);
        // Fake user creation
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserAccount>(), It.IsAny<string>())).Callback<UserAccount, string>((u, p) => u.Id = userId).ReturnsAsync(IdentityResult.Success);

        var result = await _service.CreateUserAsync("test@example.com", "password");

        Assert.That(result.Result.Succeeded, Is.True);
        Assert.That(result.UserId, Is.EqualTo(userId));
    }

    [Test]
    public void CreateUserAsync_Failed_ThrowsServerInternalError()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("fail@example.com")).ReturnsAsync((UserAccount)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserAccount>(), "password")).ReturnsAsync(IdentityResult.Failed());

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.CreateUserAsync("fail@example.com", "password"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.COMMON_SERVER_INTERNAL_ERROR));
    }
} 
