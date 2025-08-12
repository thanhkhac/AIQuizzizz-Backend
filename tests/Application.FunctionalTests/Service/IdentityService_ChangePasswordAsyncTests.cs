using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Application.Command.UnitTests.Service;

[TestFixture]
public class IdentityService_ChangePasswordAsyncTests : IdentityServiceTestBase
{
    [Test]
    public void ChangePasswordAsync_UserNotFound_ThrowsAccountNotFound()
    {
        //Precondition: Không có user nào với userId này trong hệ thống
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((UserAccount)null);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ChangePasswordAsync(userId, "oldpassword", "newpassword"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_NOTFOUND));
    }

    [Test]
    public void ChangePasswordAsync_WrongPassword_ThrowsAccountWrongPassword()
    {
        //Precondition: User tồn tại, nhập sai mật khẩu cũ
        var userId = Guid.NewGuid();
        var user = new UserAccount
        {
            Id = userId
        };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        var identityError = new IdentityError
        {
            Code = nameof(IdentityErrorDescriber.PasswordMismatch)
        };
        var result = IdentityResult.Failed(identityError);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "oldpassword", "newpassword")).ReturnsAsync(result);

        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _service.ChangePasswordAsync(userId, "oldpassword", "newpassword"));
        Assert.That(ex.Errors, Does.ContainKey(ErrorCodes.ACCOUNT_WRONG_PASSWORD));
    }


    [Test]
    public async Task ChangePasswordAsync_Success()
    {
        //Precondition: User tồn tại, đổi mật khẩu thành công
        var userId = Guid.NewGuid();
        var user = new UserAccount
        {
            Id = userId
        };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "oldpassword", "newpassword")).ReturnsAsync(IdentityResult.Success);

        await _service.ChangePasswordAsync(userId, "oldpassword", "newpassword");

        _userManagerMock.Verify(x => x.ChangePasswordAsync(user, "oldpassword", "newpassword"), Times.Once);
    }
}
