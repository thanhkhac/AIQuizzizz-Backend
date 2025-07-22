using NUnit.Framework;
using System;
using System.Threading.Tasks;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_IsInAnyRoleAsyncTests : IdentityServiceTestDatabaseContainerBase
{
    [Test]
    public async Task IsInAnyRoleAsync_UserInRole_ReturnsTrue()
    {
        // Precondition: Seed user và role "Admin" vào database, gán user vào role
        var userId = Guid.NewGuid();
        var user = new UserAccount
        {
            Id = userId,
            UserName = "testuser2",
            Email = "testuser2@example.com",
            EmailConfirmed = true
        };
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>();
        var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // Tạo role nếu chưa có
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
        }

        // Tạo user nếu chưa có
        var userInDb = await userManager.FindByIdAsync(userId.ToString());
        if (userInDb == null)
        {
            await userManager.CreateAsync(user, "Password123!");
        }

        // Gán user vào role
        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }

        // Thực hiện test
        var result = await _service.IsInAnyRoleAsync(userId, "Admin");
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsInAnyRoleAsync_UserNotInRole_ReturnsFalse()
    {
        // Precondition: Seed user nhưng không gán vào role
        var userId = Guid.NewGuid();
        var user = new UserAccount
        {
            Id = userId,
            UserName = "testuser3",
            Email = "testuser3@example.com",
            EmailConfirmed = true
        };
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>();
        var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // Tạo role nếu chưa có
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
        }

        // Tạo user nếu chưa có
        var userInDb = await userManager.FindByIdAsync(userId.ToString());
        if (userInDb == null)
        {
            await userManager.CreateAsync(user, "Password123!");
        }

        // Đảm bảo user không ở trong role
        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.RemoveFromRoleAsync(user, "Admin");
        }

        // Thực hiện test
        var result = await _service.IsInAnyRoleAsync(userId, "Admin");
        Assert.That(result, Is.False);
    }
}
