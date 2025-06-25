namespace CleanArchitectureBase.Application.Common.Models;

public class ApiResponse<T>
{
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    public IDictionary<string, string[]> ValidationErrors { get; set; } = new Dictionary<string, string[]>();
    public T? Data { get; set; }
    public bool Success { get; set; }
    
    
    public static ApiResponse<T> SuccessResult(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }
}


public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResult()
    {
        return new ApiResponse
        {
            Success = true
        };
    }
}
