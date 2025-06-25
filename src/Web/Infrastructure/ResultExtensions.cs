using CleanArchitectureBase.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitectureBase.Web.Infrastructure;

public static class ResultExtensions
{
    public static Ok<ApiResponse<T>> ToOk<T>(this T data, string message = "Request processed successfully")
    {
        return TypedResults.Ok(ApiResponse<T>.SuccessResult(data));
    }
    public static Ok<ApiResponse> ToOk(this ApiResponse response)
    {
        return TypedResults.Ok(response);
    }
    
}
