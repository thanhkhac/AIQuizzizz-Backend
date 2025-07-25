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
    protected Mock<RoleManager<ApplicationRole>> _roleManagerMock = IdentityTestHelpers.MockRoleManager<ApplicationRole>();

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

        var refreshTokenDbSetMock = CreateMockDbSet<RefreshToken>();
        _dbContextMock.Setup(x => x.Set<RefreshToken>()).Returns(refreshTokenDbSetMock.Object);

        _service = new IdentityService(
            _userManagerMock.Object,
            _claimsFactoryMock.Object,
            _authServiceMock.Object,
            _signInManagerMock.Object,
            _jwtOptionsMock.Object,
            _dbContextMock.Object,
            _googleAuthServiceMock.Object,
            _emailServiceMock.Object,
            _roleManagerMock.Object
        );
    }
    
    public static Mock<DbSet<T>> CreateMockDbSet<T>(params T[] entities) where T : class
    {
        var list = entities.ToList();
        var queryable = list.AsQueryable();

        var mockSet = new Mock<DbSet<T>>();

        // Hỗ trợ LINQ query
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(() => list.AsQueryable().Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(() => list.AsQueryable().Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(() => list.AsQueryable().ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => list.AsQueryable().GetEnumerator());

        // Hỗ trợ Add
        mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(entity => list.Add(entity));
        mockSet.Setup(d => d.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(entities => list.AddRange(entities));

        // Hỗ trợ Remove
        mockSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>(entity => list.Remove(entity));
        mockSet.Setup(d => d.RemoveRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(entities =>
        {
            foreach (var entity in entities)
            {
                list.Remove(entity);
            }
        });

       return mockSet;
    }

    
    public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> queryableData) where T : class
    {
        var list = queryableData.ToList(); // chuyển về list để hỗ trợ Add/Remove
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(() => list.AsQueryable().Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(() => list.AsQueryable().Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(() => list.AsQueryable().ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => list.AsQueryable().GetEnumerator());

        mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(entity => list.Add(entity));
        mockSet.Setup(d => d.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(entities => list.AddRange(entities));

        mockSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>(entity => list.Remove(entity));
        mockSet.Setup(d => d.RemoveRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(entities =>
        {
            foreach (var entity in entities)
            {
                list.Remove(entity);
            }
        });

        return mockSet;
    }



}
