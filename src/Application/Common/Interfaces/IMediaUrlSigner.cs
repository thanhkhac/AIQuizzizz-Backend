namespace CleanArchitectureBase.Application.Common.Interfaces;

/// <summary>
/// Tạo URL tải media có chữ ký, hết hạn sau thời gian ngắn (bucket MinIO là private)
/// </summary>
public interface IMediaUrlSigner
{
    string GetUrl(string objectKey, TimeSpan lifetime);
}
