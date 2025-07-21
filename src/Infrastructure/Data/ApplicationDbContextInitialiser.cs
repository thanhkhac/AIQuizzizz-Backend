using System.Runtime.InteropServices;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureBase.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        
        await initialiser.SeedAsync();
        await Task.CompletedTask;
    }
}

//Vai trò: Khởi tạo và nạp dữ liệu cho cơ sở dữ liệu
public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<UserAccount> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context,
        UserManager<UserAccount> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            //OPTION: Xóa database hiện tại
            // await _context.Database.EnsureDeletedAsync();
            //Thực hiện các migrations chưa được áp dụng
            // var databaseExists = await _context.Database.EnsureCreatedAsync();

            // if (!databaseExists)
            // {
            //await _context.Database.MigrateAsync();
            // }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new ApplicationRole(Roles.Administrator);
        var moderatorRole = new ApplicationRole(Roles.Moderator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }
        
        if (_roleManager.Roles.All(r => r.Name != moderatorRole.Name))
        {
            await _roleManager.CreateAsync(moderatorRole);
        }

        // Default users
        var user = new User { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), FullName = "Admin", Email = "sa@gmail.com", IsBanned = false };
        var administrator = new UserAccount
        {
            Id = user.Id,
            UserName = "77777777-7777-7777-7777-777777777777",
            Email = "sa@gmail.com",
            IsDeleted = false,
            User = user,
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Sa@1234");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }

        // Default data
        // Seed, if necessary
        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.Add(new TodoList
            {
                Title = "Todo List",
                Items =
                {
                    new TodoItem { Title = "Make a todo list 📃" },
                    new TodoItem { Title = "Check off the first item ✅" },
                    new TodoItem { Title = "Realise you've already done two things on the list! 🤯" },
                    new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                }
            });

            await _context.SaveChangesAsync();
        }

        // Seed Plan
        if (!_context.Set<Plan>().Any())
        {
            var plan1 = new Plan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Cơ Bản",
                Price = 0,
                DayDuration = 30,
                CanLearn = true,
                CanOpenTest = false,
                CanCopyOrImportQuestionSet = false,
                IsDeleted = false
            };
            var plan2 = new Plan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Nâng Cao",
                Price = 199000,
                DayDuration = 90,
                CanLearn = true,
                CanOpenTest = true,
                CanCopyOrImportQuestionSet = true,
                IsDeleted = false
            };
            _context.Set<Plan>().AddRange(plan1, plan2);
            await _context.SaveChangesAsync();
        }

        // Seed TokenPackage
        if (!_context.Set<TokenPackage>().Any())
        {
            var package1 = new TokenPackage
            {
                Id = Guid.NewGuid(),
                Name = "Gói 10 Token",
                Price = 10000,
                TokenCount = 10,
                IsDeleted = false
            };
            var package2 = new TokenPackage
            {
                Id = Guid.NewGuid(),
                Name = "Gói 50 Token",
                Price = 45000,
                TokenCount = 50,
                IsDeleted = false
            };
            _context.Set<TokenPackage>().AddRange(package1, package2);
            await _context.SaveChangesAsync();
        }

        // Seed UserTokenPurchase (ví dụ cho admin)
        var adminUser = await _userManager.FindByEmailAsync("sa@gmail.com");
        var tokenPackage = _context.Set<TokenPackage>().FirstOrDefault();
        if (adminUser != null && tokenPackage != null && !_context.Set<UserTokenPurchase>().Any())
        {
            var purchase = new UserTokenPurchase
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                TokenPackageId = tokenPackage.Id,
                TokenPackage = tokenPackage,
                User = adminUser.User
            };
            _context.Set<UserTokenPurchase>().Add(purchase);
            await _context.SaveChangesAsync();
        }

        // Seed UserSubscription (ví dụ cho admin)
        var plan = _context.Set<Plan>().FirstOrDefault();
        if (adminUser != null && plan != null && !_context.Set<UserSubscription>().Any())
        {
            var subscription = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                PlanId = plan.Id,
                DateStart = DateTimeOffset.UtcNow,
                DateFinish = DateTimeOffset.UtcNow.AddDays(plan.DayDuration),
                IsActive = true,
                User = adminUser.User,
                Plan = plan
            };
            _context.Set<UserSubscription>().Add(subscription);
            await _context.SaveChangesAsync();
        }

        // Seed thêm các tài khoản Lecturer, Student, Moderator
        var lecturerUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Lecturer",
            Email = "lecturer@gmail.com",
            IsBanned = false
        };
        var lecturerAccount = new UserAccount
        {
            Id = lecturerUser.Id,
            UserName = lecturerUser.Id.ToString(),
            Email = "lecturer@gmail.com",
            IsDeleted = false,
            User = lecturerUser,
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.UserName != lecturerAccount.UserName))
        {
            await _userManager.CreateAsync(lecturerAccount, "123456");
        }

        var studentUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Student",
            Email = "student@gmail.com",
            IsBanned = false
        };
        var studentAccount = new UserAccount
        {
            Id = studentUser.Id,
            UserName = studentUser.Id.ToString(),
            Email = "student@gmail.com",
            IsDeleted = false,
            User = studentUser,
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.UserName != studentAccount.UserName))
        {
            await _userManager.CreateAsync(studentAccount, "123456");
        }

        var moderatorUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Moderator",
            Email = "moderator@gmail.com",
            IsBanned = false
        };
        var moderatorAccount = new UserAccount
        {
            Id = moderatorUser.Id,
            UserName = moderatorUser.Id.ToString(),
            Email = "moderator@gmail.com",
            IsDeleted = false,
            User = moderatorUser,
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.UserName != moderatorAccount.UserName))
        {
            await _userManager.CreateAsync(moderatorAccount, "123456");
            await _userManager.AddToRoleAsync(moderatorAccount, Roles.Moderator); // Nếu có Roles.Moderator
        }
    }
}
