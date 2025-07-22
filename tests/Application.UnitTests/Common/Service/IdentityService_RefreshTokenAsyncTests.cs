using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

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