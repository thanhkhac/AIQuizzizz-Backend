using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Infrastructure.Data;
using CleanArchitectureBase.Domain.Entities;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

[TestFixture]
public class IdentityService_IsInAnyRoleAsyncTests : IdentityServiceTestBase
{
    private Guid _userId;
    private Guid _adminRoleId;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _userId = Guid.NewGuid();
        _adminRoleId = Guid.NewGuid();
    }

    [Test]
    public async Task IsInAnyRoleAsync_UserInRole_ReturnsTrue()
    {
        // Precondition: User có trong role truyền vào
        var userRoles = new List<ApplicationUserRole>
        {
            new ApplicationUserRole { UserId = _userId, RoleId = _adminRoleId }
        };
        var roles = new List<ApplicationRole>
        {
            new ApplicationRole { Id = _adminRoleId, Name = "Admin" }
        };
        var userRoleSet = IdentityTestHelpers.CreateMockDbSet(userRoles);
        var roleSet = IdentityTestHelpers.CreateMockDbSet(roles);
        _dbContextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _dbContextMock.Setup(x => x.UserRoles).Returns(userRoleSet.Object);
        _dbContextMock.Setup(x => x.Roles).Returns(roleSet.Object);
        typeof(IdentityService).GetField("_dbContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_service, _dbContextMock.Object);
        var result = await _service.IsInAnyRoleAsync(_userId, "Admin");
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsInAnyRoleAsync_UserNotInRole_ReturnsFalse()
    {
        // Precondition: User không có trong role truyền vào
        var userRoles = new List<ApplicationUserRole>();
        var roles = new List<ApplicationRole>
        {
            new ApplicationRole { Id = _adminRoleId, Name = "Admin" }
        };
        var userRoleSet = IdentityTestHelpers.CreateMockDbSet(userRoles);
        var roleSet = IdentityTestHelpers.CreateMockDbSet(roles);
        _dbContextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _dbContextMock.Setup(x => x.UserRoles).Returns(userRoleSet.Object);
        _dbContextMock.Setup(x => x.Roles).Returns(roleSet.Object);
        typeof(IdentityService).GetField("_dbContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_service, _dbContextMock.Object);
        var result = await _service.IsInAnyRoleAsync(_userId, "Admin");
        Assert.That(result, Is.False);
    }
} 
