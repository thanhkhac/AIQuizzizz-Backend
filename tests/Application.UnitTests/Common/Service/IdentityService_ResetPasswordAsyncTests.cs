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

namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

[TestFixture]
public class IdentityService_ResetPasswordAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void ResetPasswordAsync_UserNotFound_ThrowsAccountNotFound()
    {
        var dto = new ResetPasswordDto
        {
            Email = "notfound@example.com",
            ResetCode = "123456",
            NewPassword = "newpass"
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ResetPasswordAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public void ResetPasswordAsync_WrongCode_ThrowsInvalidResetCode()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@example.com",
            ResetCode = "wrong",
            NewPassword = "newpass"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            PasswordResetCode = "right",
            PasswordResetCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ResetPasswordAsync(dto));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_INVALID_RESET_CODE));
    }

    [Test]
    public async Task ResetPasswordAsync_Success()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@example.com",
            ResetCode = "123456",
            NewPassword = "newpass"
        };
        var user = new UserAccount
        {
            Email = dto.Email,
            PasswordResetCode = "123456",
            PasswordResetCodeExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5)
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");
        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, "token", dto.NewPassword)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _service.ResetPasswordAsync(dto);

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.AtLeastOnce);
    }
}
