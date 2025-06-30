namespace CleanArchitectureBase.Domain.Entities;

//TODO: đổi tiền về kiểu dữ liệu khác nếu muốn sử dụng quốc tế
public class User 
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public string? FullName { get; set; } 
    public required string Email { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsBanned { get; set; }
    public long TokenCount { get; set; }
    public long Balance { get; set; }
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationCodeTime { get; set; }
    public int EmailVerificationLockout { get; set; }

    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiryTime { get; set; }
    public int PasswordResetLockout { get; set; }

    // Lockout khi gửi quá nhiều yêu cầu email (xác thực/quên mật khẩu)
    public int EmailRequestLockout { get; set; }
    public DateTime? EmailRequestLockoutTime { get; set; }
}


//TODO: bảng nạp tiền
