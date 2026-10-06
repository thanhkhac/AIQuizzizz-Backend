using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.MediaFiles.Dtos;

/// <summary>
/// Media gắn với câu hỏi trả về cho client. Url/ThumbnailUrl là presigned URL ngắn hạn: chỉ được sinh ra trong
/// response của các API đã kiểm tra quyền xem câu hỏi -> quyền xem media đi theo quyền xem câu hỏi.
/// </summary>
public class QuestionMediaDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }

    public static QuestionMediaDto? From(Guid? mediaId, MediaType? mediaType)
    {
        if (mediaId == null || mediaType == null)
            return null;

        var ttl = MediaKeys.TtlFor(mediaType.Value);
        return new QuestionMediaDto
        {
            Id = mediaId.Value,
            Type = mediaType.Value.ToString(),
            Url = MediaKeys.SignedUrl(MediaKeys.MainKey(mediaId.Value, mediaType.Value), ttl),
            ThumbnailUrl = mediaType == MediaType.Video
                ? MediaKeys.SignedUrl(MediaKeys.VideoThumbnailKey(mediaId.Value), ttl)
                : null,
            ExpiresAt = DateTimeOffset.UtcNow.Add(ttl)
        };
    }
}

/// <summary>
/// Quy ước object key trên MinIO (phải khớp với service media-ai) + ký URL.
/// Signer/TTL được gán 1 lần lúc khởi động (mapper DTO là static nên không inject được).
/// </summary>
public static class MediaKeys
{
    public static IMediaUrlSigner? Signer { get; set; }
    public static TimeSpan ImageTtl { get; set; } = TimeSpan.FromMinutes(15);
    public static TimeSpan VideoTtl { get; set; } = TimeSpan.FromMinutes(120);

    public static string MainKey(Guid id, MediaType type) =>
        type == MediaType.Image ? $"img/{id}.jpg" : $"vid/{id}.mp4";

    public static string VideoThumbnailKey(Guid id) => $"vid/{id}.jpg";

    public static TimeSpan TtlFor(MediaType type) => type == MediaType.Video ? VideoTtl : ImageTtl;

    public static string? SignedUrl(string key, TimeSpan ttl) => Signer?.GetUrl(key, ttl);
}

public class UploadMediaResultDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double? DurationSeconds { get; set; }
    public long Size { get; set; }
}

public class MediaPermissionDto
{
    public bool CanUploadImage { get; set; }
    public bool CanUploadVideo { get; set; }
    public int MaxImageSizeMb { get; set; }
    public int MaxVideoSizeMb { get; set; }
    public int MaxVideoSeconds { get; set; }
}
