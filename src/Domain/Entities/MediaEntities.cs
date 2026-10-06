namespace CleanArchitectureBase.Domain.Entities;

public enum MediaType
{
    Image,
    Video
}

public enum MediaModerationStatus
{
    Pending,      // chờ job kiểm duyệt
    Approved,     // hợp lệ
    Violated,     // vi phạm -> đã xoá file + câu hỏi
    NotRequired,  // video: hiện chưa kiểm duyệt tự động
    Failed        // kiểm duyệt lỗi quá số lần thử
}

/// <summary>
/// File media (ảnh/video) đã chuẩn hoá và lưu trên MinIO
/// </summary>
public class Media : BaseAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid OwnerId { get; set; }
    public required MediaType Type { get; set; }
    public required string ObjectKey { get; set; }
    public string? ThumbnailKey { get; set; }
    public required string ContentType { get; set; }
    public long Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double? DurationSeconds { get; set; }
    public string? OriginalFileName { get; set; }

    public MediaModerationStatus ModerationStatus { get; set; } = MediaModerationStatus.Pending;
    public string? ModerationResultJson { get; set; }
    public DateTimeOffset? ModeratedAt { get; set; }
    public int ModerationAttempts { get; set; }
    public bool IsDeleted { get; set; }

    public User? Owner { get; set; }
    public List<Question> Questions { get; set; } = new();
}

/// <summary>
/// Vi phạm tiêu chuẩn cộng đồng (giống "strike" của YouTube): hết hạn sau một khoảng thời gian
/// </summary>
public class UserViolation : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid UserId { get; set; }
    public Guid? MediaId { get; set; }
    public required string Reason { get; set; }
    public string? DetailJson { get; set; }
    public int DeletedQuestionCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public required DateTimeOffset ExpiresAt { get; set; }
    public bool IsExpired { get; set; }

    public User? User { get; set; }
}
