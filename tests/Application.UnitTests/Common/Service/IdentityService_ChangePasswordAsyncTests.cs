using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CleanArchitectureBase.Infrastructure.Identity;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.UnitTests.Common.Service;

[TestFixture]
public class IdentityService_ChangePasswordAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void ChangePasswordAsync_UserNotFound_ThrowsAccountNotFound()
    {
        // Precondition: Không có user nào với userId này trong hệ thống
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ChangePasswordAsync(userId, "old", "new"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public void ChangePasswordAsync_WrongPassword_ThrowsAccountWrongPassword()
    {
        // Precondition: User tồn tại, nhập sai mật khẩu cũ
        var userId = Guid.NewGuid();
        var user = new UserAccount { Id = userId };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        var identityError = new IdentityError { Code = nameof(IdentityErrorDescriber.PasswordMismatch) };
        var result = IdentityResult.Failed(identityError);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "old", "new")).ReturnsAsync(result);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ChangePasswordAsync(userId, "old", "new"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_WRONG_PASSWORD));
    }

    [Test]
    public void ChangePasswordAsync_Failed_ThrowsServerInternalError()
    {
        // Precondition: User tồn tại, đổi mật khẩu thất bại vì lý do khác
        var userId = Guid.NewGuid();
        var user = new UserAccount { Id = userId };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        var identityError = new IdentityError { Code = "OtherError" };
        var result = IdentityResult.Failed(identityError);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "old", "new")).ReturnsAsync(result);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ChangePasswordAsync(userId, "old", "new"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.COMMON_SERVER_INTERNAL_ERROR));
    }

    [Test]
    public async Task ChangePasswordAsync_Success()
    {
        // Precondition: User tồn tại, đổi mật khẩu thành công
        var userId = Guid.NewGuid();
        var user = new UserAccount { Id = userId };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "old", "new")).ReturnsAsync(IdentityResult.Success);

        await _service.ChangePasswordAsync(userId, "old", "new");

        _userManagerMock.Verify(x => x.ChangePasswordAsync(user, "old", "new"), Times.Once);
    }
} 