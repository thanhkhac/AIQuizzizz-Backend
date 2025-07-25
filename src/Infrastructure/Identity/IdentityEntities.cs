using System.ComponentModel.DataAnnotations;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Infrastructure.Identity;

//TODO: Đưa về thống nhất về access fail và lockoutend 
public class UserAccount : IdentityUser<Guid>
{
    public bool IsDeleted { get; set; }
    public bool IsBanned { get; set; }
    
    [StringLength(10)]
    public string? PasswordResetCode { get; set; }

    public DateTimeOffset? PasswordResetCodeExpiryTime { get; set; }
    public int FailedPasswordResetAttempts { get; set; } = 0;
    public DateTimeOffset? PasswordResetLockoutEnd { get; set; }
    public int PasswordResetRequestAttempts { get; set; } = 0;
    public DateTimeOffset? PasswordResetRequestLockoutEnd { get; set; }

    [StringLength(10)]
    public string? EmailVerificationCode { get; set; }
    public DateTimeOffset? EmailVerificationCodeExpiryTime { get; set; }
    public int FailedEmailVerificationAttempts { get; set; } = 0;
    public DateTimeOffset? EmailVerificationLockoutEnd { get; set; }
    public int EmailVerificationRequestAttempts { get; set; } = 0;
    public DateTimeOffset? EmailVerificationRequestLockoutEnd { get; set; }

    // Lockout khi gửi quá nhiều yêu cầu email 
    public int EmailRequestLockout { get; set; } = 0;
    public DateTime? EmailRequestLockoutTime { get; set; }

    public User? User { get; set; }
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
