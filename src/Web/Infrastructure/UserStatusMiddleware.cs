using System.Security.Claims;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Infrastructure.Identity;

namespace CleanArchitectureBase.Web.Infrastructure;

/// <summary>
/// Từ chối request của user đã bị khoá / xoá dù access token vẫn còn hạn (401, cùng format ApiResponse).
/// </summary>
public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserStatusService userStatusService)
    {
        var userIdValue = context.User?.Identity?.IsAuthenticated == true
            ? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

        if (Guid.TryParse(userIdValue, out var userId))
        {
            var status = await userStatusService.GetStatusAsync(userId, context.RequestAborted);
            if (status.IsBlocked)
            {
                var code = status.Exists && !status.IsDeleted && status.IsBanned
                    ? ErrorCodes.ACCOUNT_BANNED
                    : ErrorCodes.COMMON_UNAUTHORIZED;
                var message = code == ErrorCodes.ACCOUNT_BANNED ? status.BanReason ?? "" : "Unauthorized";

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Errors = new Dictionary<string, string[]> { [code] = new[] { message } }
                });
                return;
            }
        }

        await _next(context);
    }
}
