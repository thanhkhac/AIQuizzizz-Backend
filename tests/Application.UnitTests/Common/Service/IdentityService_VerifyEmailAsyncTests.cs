using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Application.Users.Common;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

[TestFixture]
public class IdentityService_VerifyEmailAsyncTests : IdentityServiceTestBase
{
    
    [Test]
    public void VerifyEmailAsync_UserNotFound_ThrowsAccountNotFound()
    {
        var dto = new EmailVerificationConfirmDto { Email = "notfound@example.com", VerificationCode = "123456" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.VerifyEmailAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public void VerifyEmailAsync_WrongCode_ThrowsInvalidVerificationCode()
    {
        var dto = new EmailVerificationConfirmDto { Email = "user@example.com", VerificationCode = "wrong" };
        var user = new UserAccount { Email = dto.Email, EmailVerificationCode = "right", EmailVerificationCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5) };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.VerifyEmailAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_VERIFICATION_CODE));
    }

    [Test]
    public async Task VerifyEmailAsync_Success()
    {
        var dto = new EmailVerificationConfirmDto { Email = "user@example.com", VerificationCode = "123456" };
        var user = new UserAccount
        {
            Email = dto.Email,
            EmailVerificationCode = "123456",
            EmailVerificationCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _service.VerifyEmailAsync(dto);

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.AtLeastOnce);
        Assert.That(user.EmailConfirmed, Is.True);
    }
} 
