using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_RefreshTokenAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void RefreshTokenAsync_RefreshTokenNotFound_ThrowsInvalidCredentials()
    {
        // Giả lập DbSet<RefreshToken> rỗng
        var mockSet = new Mock<DbSet<RefreshToken>>();
        var dbContextMock = new Mock<CleanArchitectureBase.Infrastructure.Data.ApplicationDbContext>(new DbContextOptions<CleanArchitectureBase.Infrastructure.Data.ApplicationDbContext>());
        dbContextMock.Setup(x => x.Set<RefreshToken>()).Returns(mockSet.Object);
        // inject lại dbContext vào _service nếu cần
        // ...
        // Gọi hàm và kiểm tra exception
        // ...
    }
} 
