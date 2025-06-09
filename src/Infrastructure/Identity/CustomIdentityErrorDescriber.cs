using Microsoft.AspNetCore.Identity;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Infrastructure.Identity;

public class CustomIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_DEFAULT_ERROR,
            Description = "Đã xảy ra lỗi không xác định."
        };
    }

    public override IdentityError ConcurrencyFailure()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_CONCURRENCY_FAILURE,
            Description = "Lỗi đồng bộ hóa, dữ liệu đã bị thay đổi bởi người dùng khác."
        };
    }

    public override IdentityError PasswordMismatch()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_PASSWORD_MISMATCH,
            Description = "Mật khẩu không khớp."
        };
    }

    public override IdentityError InvalidToken()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_INVALID_TOKEN,
            Description = "Token không hợp lệ."
        };
    }

    public override IdentityError RecoveryCodeRedemptionFailed()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_RECOVERY_CODE_REDEMPTION_FAILED,
            Description = "Không thể sử dụng mã khôi phục."
        };
    }

    public override IdentityError LoginAlreadyAssociated()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_LOGIN_ALREADY_ASSOCIATED,
            Description = "Tài khoản đăng nhập bên ngoài đã được liên kết với một người dùng khác."
        };
    }

    public override IdentityError InvalidUserName(string? userName)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_INVALID_USER_NAME,
            Description = $"Tên người dùng '{userName}' không hợp lệ."
        };
    }

    public override IdentityError InvalidEmail(string? email)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_INVALID_EMAIL,
            Description = $"Email '{email}' không hợp lệ."
        };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_DUPLICATE_USER_NAME,
            Description = $"Tên người dùng '{userName}' đã được sử dụng."
        };
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_DUPLICATE_EMAIL,
            Description = $"Email '{email}' đã được sử dụng."
        };
    }

    public override IdentityError InvalidRoleName(string? role)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_INVALID_ROLE_NAME,
            Description = $"Tên vai trò '{role}' không hợp lệ."
        };
    }

    public override IdentityError DuplicateRoleName(string role)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_DUPLICATE_ROLE_NAME,
            Description = $"Tên vai trò '{role}' đã tồn tại."
        };
    }

    public override IdentityError UserAlreadyHasPassword()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_USER_ALREADY_HAS_PASSWORD,
            Description = "Người dùng đã có mật khẩu."
        };
    }

    public override IdentityError UserLockoutNotEnabled()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_USER_LOCKOUT_NOT_ENABLED,
            Description = "Khóa tài khoản người dùng chưa được kích hoạt."
        };
    }

    public override IdentityError UserAlreadyInRole(string role)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_USER_ALREADY_IN_ROLE,
            Description = $"Người dùng đã thuộc vai trò '{role}'."
        };
    }

    public override IdentityError UserNotInRole(string role)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_USER_NOT_IN_ROLE,
            Description = $"Người dùng không thuộc vai trò '{role}'."
        };
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_PASSWORD_TOO_SHORT,
            Description = $"Mật khẩu phải có ít nhất {length} ký tự."
        };
    }

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_PASSWORD_REQUIRES_UNIQUE_CHARS,
            Description = $"Mật khẩu phải chứa ít nhất {uniqueChars} ký tự khác nhau."
        };
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_PASSWORD_REQUIRES_NON_ALPHANUMERIC,
            Description = "Mật khẩu phải chứa ít nhất một ký tự không phải chữ hoặc số."
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_PASSWORD_REQUIRES_DIGIT,
            Description = "Mật khẩu phải chứa ít nhất một chữ số."
        };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_PASSWORD_REQUIRES_LOWER,
            Description = "Mật khẩu phải chứa ít nhất một chữ cái thường."
        };
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = ErrorCodes.IDENTITY_PASSWORD_REQUIRES_UPPER,
            Description = "Mật khẩu phải chứa ít nhất một chữ cái in hoa."
        };
    }
}
