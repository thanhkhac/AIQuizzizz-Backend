namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface  IRedisService
{
    Task SetStringAsync(string key, string value, TimeSpan? expiry = null); // Thêm cặp key-value (String)
    Task<string?> GetStringAsync(string key); // Lấy data theo key (String)
    Task DeleteKeyAsync(string key); // Xóa theo key
    Task ListLeftPush(string key, string value); // Thêm data danh sách
    Task<List<string>> ListRangeAsync(string key); // Lấy list theo key
}
