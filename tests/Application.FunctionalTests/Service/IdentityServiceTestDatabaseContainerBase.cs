using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.FunctionalTests;
using CleanArchitectureBase.Infrastructure.Data;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

using static Testing;

public class IdentityServiceTestDatabaseContainerBase : BaseTestFixture
{

    protected IdentityService _service;
    protected ApplicationDbContext _dbContext;

    protected IServiceScope _scope;

    [SetUp]
    public override async Task TestSetUp()
    {

        await base.TestSetUp();
        
        if (_scopeFactory == null)
            throw new InvalidOperationException("_scopeFactory is not initialized. Ensure Testing.OneTimeSetUp is completed.");
            
        _scope = CreateScope();

        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>();
        var _claimsFactory = _scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<UserAccount>>();
        var _authService = _scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var _signInManager = _scope.ServiceProvider.GetRequiredService<SignInManager<UserAccount>>();
        var _roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var jwtSettings = new JwtSettings
        {
            SecretKey = "12345678901234567890123456789012345678901234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        };
        var _jwtOptions = Microsoft.Extensions.Options.Options.Create(jwtSettings);

        var _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        var _emailServiceMock = new Mock<IEmailService>();

        _service = new IdentityService(
            _userManager, 
            _claimsFactory, 
            _authService, 
            _signInManager, 
            _jwtOptions, 
            _dbContext, 
            _googleAuthServiceMock.Object, 
            _emailServiceMock.Object ,
            _roleManager
        );
    }
    
    [TearDown]
    public void DisposeScope()
    {
        _scope?.Dispose();
        _dbContext?.Dispose();
    }

}
