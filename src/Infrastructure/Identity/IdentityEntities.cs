using System.ComponentModel.DataAnnotations;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Infrastructure.Identity;

public class UserAccount : IdentityUser<Guid>
{
    public bool IsDeleted { get; set; }
    public bool IsBanned { get; set; }
    
    [StringLength(10)]
    public string? PasswordResetCode { get; set; }

    public DateTime? PasswordResetCodeExpiryTime { get; set; }
        
    public int PasswordResetLockout { get; set; } = 0;
    
    [StringLength(10)]
    public string? EmailVerificationCode { get; set; }

    public DateTime? EmailVerificationCodeTime { get; set; }
        
    public int EmailVerificationLockout { get; set; } = 0;

    // Lockout khi gửi quá nhiều yêu cầu email (xác thực/quên mật khẩu)
    public int EmailRequestLockout { get; set; } = 0;
    public DateTime? EmailRequestLockoutTime { get; set; }

    public required User User { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() => this.Id = Guid.NewGuid();

    public ApplicationRole(string roleName)
        : this()
    {
        this.Name = roleName;
    }
}

public class ApplicationUserClaim : IdentityUserClaim<Guid>;

public class ApplicationUserLogin : IdentityUserLogin<Guid>;

public class ApplicationUserToken : IdentityUserToken<Guid>;

public class ApplicationUserRole : IdentityUserRole<Guid>;

public class ApplicationRoleClaim : IdentityRoleClaim<Guid>;
