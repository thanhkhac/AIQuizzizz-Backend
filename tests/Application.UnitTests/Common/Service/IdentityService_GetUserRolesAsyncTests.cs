using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Domain.Entities;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

[TestFixture]
public class IdentityService_GetUserRolesAsyncTests : IdentityServiceTestBase
{
    [Test]
    public async Task GetUserRolesAsync_UserNotFound_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var result = await _service.GetUserRolesAsync(userId);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetUserRolesAsync_UserExists_ReturnsRoles()
    {
        var userId = Guid.NewGuid();
        var user = new UserAccount { Id = userId };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin", "User" });

        var result = await _service.GetUserRolesAsync(userId);

        Assert.That(result, Is.EquivalentTo(new[] { "Admin", "User" }));
    }
} 
