using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Infrastructure.Data;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

public class IdentityServiceTestBase
{
    protected Mock<UserManager<UserAccount>> _userManagerMock;
    protected Mock<IUserClaimsPrincipalFactory<UserAccount>> _claimsFactoryMock;
    protected Mock<IAuthorizationService> _authServiceMock;
    protected Mock<SignInManager<UserAccount>> _signInManagerMock;
    protected Mock<IOptions<JwtSettings>> _jwtOptionsMock;
    protected Mock<IGoogleAuthService> _googleAuthServiceMock;
    protected Mock<IEmailService> _emailServiceMock;
    protected IdentityService _service;
    protected Mock<ApplicationDbContext> _dbContextMock;

    [SetUp]
    public virtual void SetUp()
    {
        _userManagerMock = IdentityTestHelpers.MockUserManager<UserAccount>();
        _claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<UserAccount>>();
        _authServiceMock = new Mock<IAuthorizationService>();
        _signInManagerMock = IdentityTestHelpers.MockSignInManager<UserAccount>(_userManagerMock);
        _jwtOptionsMock = new Mock<IOptions<JwtSettings>>();
        _dbContextMock= new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _jwtOptionsMock.Setup(x => x.Value).Returns(new JwtSettings
        {
            SecretKey = "12345678901234567890123456789012",
            Issuer = "issuer",
            Audience = "aud",
            ExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        });
        _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        _emailServiceMock = new Mock<IEmailService>();

        _service = new IdentityService(
            _userManagerMock.Object,
            _claimsFactoryMock.Object,
            _authServiceMock.Object,
            _signInManagerMock.Object,
            _jwtOptionsMock.Object,
            _dbContextMock.Object,
            _googleAuthServiceMock.Object,
            _emailServiceMock.Object
        );
    }
    

}
