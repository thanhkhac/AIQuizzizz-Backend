using System.Security.Claims;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

// import helper

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

[TestFixture]
public class IdentityService_AuthorizeAsyncTests : IdentityServiceTestBase
{

    [Test]
    public async Task AuthorizeAsync_UserExists_ReturnsAuthorizationResult()
    {
        var userId = Guid.NewGuid();
        var user = new UserAccount { Id = userId };
        var principal = new ClaimsPrincipal();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _claimsFactoryMock.Setup(x => x.CreateAsync(user)).ReturnsAsync(principal);
        _authServiceMock.Setup(x => x.AuthorizeAsync(principal, null, "Policy")).ReturnsAsync(AuthorizationResult.Success());

        var result = await _service.AuthorizeAsync(userId, "Policy");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task AuthorizeAsync_UserNotFound_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var result = await _service.AuthorizeAsync(userId, "Policy");

        Assert.That(result, Is.False);
    }
} 
