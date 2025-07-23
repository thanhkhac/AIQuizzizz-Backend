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
    // Only mock external services

    // Real services using existing Testing infrastructure
    protected IdentityService _service;
    protected ApplicationDbContext _dbContext;

    // Service scope for accessing DI services
    protected IServiceScope _scope;

    [SetUp]
    public override async Task TestSetUp()
    {
        // Call base setup first (this will handle ResetState())
        await base.TestSetUp();
        
        if (_scopeFactory == null)
            throw new InvalidOperationException("_scopeFactory is not initialized. Ensure Testing.OneTimeSetUp is completed.");
            
        // Create service scope using existing Testing infrastructure
        _scope = CreateScope();

        // Get real services from the existing DI container
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>();
        var _claimsFactory = _scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<UserAccount>>();
        var _authService = _scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var _signInManager = _scope.ServiceProvider.GetRequiredService<SignInManager<UserAccount>>();

        // Create real JWT options for testing
        var jwtSettings = new JwtSettings
        {
            SecretKey = "12345678901234567890123456789012345678901234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        };
        var _jwtOptions = Microsoft.Extensions.Options.Options.Create(jwtSettings);

        // Setup only external service mocks
        var _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        var _emailServiceMock = new Mock<IEmailService>();

        // Create service with all real dependencies except external services
        _service = new IdentityService(
            _userManager, // Real UserManager with database
            _claimsFactory, // Real IUserClaimsPrincipalFactory
            _authService, // Real IAuthorizationService
            _signInManager, // Real SignInManager with database
            _jwtOptions, // Real IOptions<JwtSettings>
            _dbContext, // Real DbContext with database
            _googleAuthServiceMock.Object, // Mocked external service
            _emailServiceMock.Object // Mocked external service
        );
    }
    
    [TearDown]
    public void DisposeScope()
    {
        _scope?.Dispose();
        _dbContext?.Dispose();
    }

}
