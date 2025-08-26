    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using CleanArchitectureBase.Application.FunctionalTests;
    using CleanArchitectureBase.Domain.Entities;
    using CleanArchitectureBase.Infrastructure.Data;
    using CleanArchitectureBase.Infrastructure.Identity;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    namespace CleanArchitectureBase.Application.Command.UnitTests;

    [SetUpFixture]
    public partial class Testing
    {
        private static ITestDatabase _database = null!;
        internal static CustomWebApplicationFactory _factory = null!;
        internal static IServiceScopeFactory _scopeFactory = null!;
        private static Guid? _userId;


        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            _database = await TestDatabaseFactory.CreateAsync();

            _factory = new CustomWebApplicationFactory(_database.GetConnection(), _database.GetConnectionString());

            _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        }

        public static IServiceScope CreateScope() => _scopeFactory?.CreateScope()
                                                     ?? throw new InvalidOperationException("Testing is not initialized. Ensure OneTimeSetUp ran.");

        public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            PrintJson("=================SendAsync=================");
            PrintJson(request);

            try
            {
                var result = await mediator.Send(request);

                PrintJson("=================Result=================");
                if (result != null)
                {
                    PrintJson(result);
                }
                return result;
            }
            catch (CleanArchitectureBase.Application.Common.Exceptions.ErrorCodeException ex)
            {
                PrintJson("=================ErrorCodeException=================");
                PrintJson(new
                {
                    Message = ex.Message,
                    Errors = ex.Errors,
                    ValidationErrors = ex.ValidationErrors,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });

                Console.WriteLine("=================Error Summary=================");
                if (ex.Errors.Any())
                {
                    Console.WriteLine("Error Codes:");
                    foreach (var error in ex.Errors)
                    {
                        Console.WriteLine($"  - {error.Key}: {string.Join(", ", error.Value)}");
                    }
                }

                if (ex.ValidationErrors.Any())
                {
                    Console.WriteLine("Validation Errors:");
                    foreach (var validationError in ex.ValidationErrors)
                    {
                        Console.WriteLine($"  - {validationError.Key}: {string.Join(", ", validationError.Value)}");
                    }
                }
                Console.WriteLine("===============================================");

                // Re-throw để test vẫn fail như expected
                throw;
            }
            catch (Exception ex)
            {
                PrintJson("=================Exception=================");
                PrintJson(new
                {
                    ExceptionType = ex.GetType().Name,
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });

                throw;
            }
        }

        public static async Task SendAsync(IBaseRequest request)
        {
            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            await mediator.Send(request);
        }

        public static Guid? GetUserId()
        {
            return _userId;
        }

        public static void Logout()
        {
            _userId = null;
        }

        public static async Task<Guid> RunAsDefaultUserAsync(int number = 0)
        {
            return await RunAsUserAsync($"test@local{number}", "Testing1234!", Array.Empty<string>());
        }
        
        

        public static async Task<Guid> RunAsAdministratorAsync()
        {
            return await RunAsUserAsync("administrator@local", "Administrator1234!", new[]
            {
                Domain.Constants.Roles.Administrator
            });
        }

        public static async Task<Guid> RunAsUserAsync(string email, string password, string[] roles)
        {
            using var scope = _scopeFactory.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>();

            // Kiểm tra user đã tồn tại chưa
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                // User đã tồn tại, chỉ cần cập nhật roles nếu cần
                if (roles.Any())
                {
                    await UpdateUserRolesAsync(userManager, scope, existingUser, roles);
                }

                _userId = existingUser.Id;
                return (Guid)_userId;
            }

            // Tạo user mới chỉ khi chưa tồn tại
            var user = new User
            {
                FullName = "Random",
                Email = email,
                Id = Guid.NewGuid()
            };
            var userAccount = new UserAccount
            {
                Id = user.Id,
                UserName = email,
                Email = email,
                User = user
            };

            var result = await userManager.CreateAsync(userAccount, password);

            if (roles.Any())
            {
                await CreateAndAssignRolesAsync(userManager, scope, userAccount, roles);
            }

            if (result.Succeeded)
            {
                _userId = userAccount.Id;
                return (Guid)_userId;
            }

            var errors = string.Join(Environment.NewLine, result.ToApplicationResult().Errors);
            throw new Exception($"Unable to create {email}.{Environment.NewLine}{errors}");
        }

        private static async Task UpdateUserRolesAsync(UserManager<UserAccount> userManager, IServiceScope scope, UserAccount user, string[] roles)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // Tạo roles nếu chưa tồn tại
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole(role));
                }
            }

            // Lấy roles hiện tại của user
            var currentRoles = await userManager.GetRolesAsync(user);

            // Chỉ thêm roles mới (tránh duplicate)
            var rolesToAdd = roles.Except(currentRoles).ToArray();
            if (rolesToAdd.Any())
            {
                await userManager.AddToRolesAsync(user, rolesToAdd);
            }
        }

        private static async Task CreateAndAssignRolesAsync(UserManager<UserAccount> userManager, IServiceScope scope, UserAccount user, string[] roles)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole(role));
                }
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        public static async Task<Guid> RunAsUserWithPlanAsync(
            string[]? roles = null,
            string email = "userWithPlan@local",
            string password = "Sa@1234",
            bool canCopyOrImportQuestionSet = true,
            bool canLearn = true,
            bool canOpenTest = true)
        {
            roles ??= new string[]
            {
                "User"
            };

            var userId = await RunAsUserAsync(email, password, roles);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var plan = new Plan
            {
                Id = Guid.NewGuid(),
                Name = $"Test Plan for {email}",
                CanCopyOrImportQuestionSet = canCopyOrImportQuestionSet,
                CanLearn = canLearn,
                Price = 0,
                Duration = 1,
                Unit = "Year",
                CanOpenTest = canOpenTest
            };
            context.Plans.Add(plan);

            var userSubscription = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PlanId = plan.Id,
                DateStart = DateTimeOffset.UtcNow.AddYears(-1),
                DateFinish = DateTimeOffset.UtcNow.AddYears(1)
            };
            context.UserSubscriptions.Add(userSubscription);

            await context.SaveChangesAsync();

            return userId;
        }

        public static async Task ResetState()
        {
            try
            {
                await _database.ResetAsync();
            }
            catch (Exception)
            {
            }

            _userId = null;
        }

        public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
            where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.FindAsync<TEntity>(keyValues);
        }

        public static async Task<List<TEntity>> QueryListAsync<TEntity>(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> queryBuilder)
            where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var query = queryBuilder(context.Set<TEntity>());

            return await query.ToListAsync();
        }

        public static async Task AddAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Add(entity);

            await context.SaveChangesAsync();
        }

        public static async Task UpdateAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Update(entity);

            await context.SaveChangesAsync();
        }

        public static async Task<int> CountAsync<TEntity>() where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.Set<TEntity>().CountAsync();
        }

        public static void PrintJson(object obj)
        {
            Console.WriteLine("=================SendAsync=================");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Đăng ký converter để bỏ qua Stream
            options.Converters.Add(new StreamConverter());

            var json = JsonSerializer.Serialize(obj, options);
            Console.WriteLine(json);
        }

        public class StreamConverter : JsonConverter<Stream>
        {
            public override Stream Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => throw new NotSupportedException("Deserializing streams is not supported");

            public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
            {
                // chỉ log metadata thay vì nội dung
                writer.WriteStringValue($"Stream(length={value.Length})");
            }
        }

        [OneTimeTearDown]
        public async Task RunAfterAnyTests()
        {
            await _database.DisposeAsync();
            await _factory.DisposeAsync();
        }
    }
