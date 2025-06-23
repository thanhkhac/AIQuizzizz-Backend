using System.Text;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Data;
using CleanArchitectureBase.Infrastructure.Data.Interceptors;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitectureBase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Lấy CNS từ appsetting
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        //Cấu hình Interceptor cho Database
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        //Cấu hình Interceptor cho xử lý Event
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            // options.UseSqlServer(connectionString);
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ApplicationDbContextInitialiser>();

        // Bind JwtSettings từ appsettings.json
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

   
        #region Lưu ý AddIdentity
        //TODO: Không nên dùng AddIdentity, vì nó sẽ mặc định đăng ký AddAuthentication của nó
        //Nên dùng IDentity
        //Nếu dùng thì phải ghi đè AddAuthentication và đặt hàm AddAuthentication ở sau AddIdentity, tránh bị Identity ghi đè
        #endregion
        services
            .AddIdentity<UserAccount, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<CustomIdentityErrorDescriber>()
            .AddDefaultTokenProviders();
        ;

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
                Guard.Against.Null(jwtSettings, message: "JwtSetting not found.");
                Guard.Against.Null(jwtSettings.SecretKey, message: "Secret key not found.");
                
                var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, // Bật kiểm tra Issuer
                    ValidateAudience = false, // Bật kiểm tra Audience
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorizationBuilder();




        services.Configure<IdentityOptions>(
            options =>
            {
                options.Password.RequiredLength = 2;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            }
        );

        services.AddSingleton(TimeProvider.System);
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        return services;
    }


}
