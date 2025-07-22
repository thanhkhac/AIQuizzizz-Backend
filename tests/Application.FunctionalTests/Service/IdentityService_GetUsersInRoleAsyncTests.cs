using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_GetUsersInRoleAsyncTests : IdentityServiceTestDatabaseContainerBase
{
    [Test]
    public async Task GetUsersInRoleAsync_ReturnsUserIds()
    {
        // Precondition: Seed user và role "Administrator" vào database
        var userId = Guid.NewGuid();
        var user = new UserAccount
        {
            Id = userId,
            UserName = "testuser",
            Email = "testuser@example.com",
            EmailConfirmed = true
        };
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>();
        var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // Tạo role nếu chưa có
        if (!await roleManager.RoleExistsAsync("Administrator"))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "Administrator" });
        }

        // Tạo user nếu chưa có
        var userInDb = await userManager.FindByIdAsync(userId.ToString());
        if (userInDb == null)
        {
            await userManager.CreateAsync(user, "Password123!");
        }

        // Gán user vào role
        if (!await userManager.IsInRoleAsync(user, "Administrator"))
        {
            await userManager.AddToRoleAsync(user, "Administrator");
        }

        // Thực hiện test
        var userIds = await _service.GetUsersInRoleAsync();
        Assert.That(userIds, Is.Not.Null);
        Assert.That(userIds, Does.Contain(userId));
    }
}
