using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Infrastructure.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Web.Endpoints;

public class Authentication : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(Login, "Login")
            .MapPost(RefreshToken, "RefreshToken")
            .MapPost(RevokeToken, "RevokeToken")
            .MapPost(LogOut, "LogOut");
    }
    
    public async Task<Ok<ApiResponse<TokenDto>>> Login([FromBody] LoginCommand command, ISender sender, HttpContext httpContext,
        IOptions<JwtSettings> jwtSettings)
    {
        var result = await sender.Send(command);

        SetTokenCookies(httpContext, result.AccessToken, result.RefreshToken, jwtSettings.Value);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<TokenDto>>> RefreshToken([FromBody] RefreshTokenCommand command,
        ISender sender,
        HttpContext httpContext,
        IOptions<JwtSettings> jwtSettings)
    {
        command.AccessToken ??= httpContext.Request.Cookies["access_token"];
        command.RefreshToken ??= httpContext.Request.Cookies["refresh_token"];

        var result = await sender.Send(command);

        //Set token vào cookie
        SetTokenCookies(httpContext, result.AccessToken, result.RefreshToken, jwtSettings.Value);

        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse>> RevokeToken([FromBody] RevokeTokenCommand command, ISender sender, HttpContext httpContext)
    {
        command.RefreshToken ??= httpContext.Request.Cookies["refresh_token"];
        await sender.Send(command);

        //Xóa các token cookie
        ClearTokenCookies(httpContext);

        return ApiResponse.SuccessResult().ToOk();
    }
    
    public Ok<ApiResponse> LogOut(HttpContext httpContext)
    {
        ClearTokenCookies(httpContext);
        return ApiResponse.SuccessResult().ToOk();
    }
    
    private void SetTokenCookies(HttpContext httpContext, string accessToken, string refreshToken, JwtSettings jwtSettings)
    {
        var expiredOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(jwtSettings.RefreshTokenExpiryDays)
        };

        httpContext.Response.Cookies.Append("access_token", accessToken, expiredOptions);
        httpContext.Response.Cookies.Append("refresh_token", refreshToken, expiredOptions);
    }
    
    private void ClearTokenCookies(HttpContext httpContext)
    {
        var expiredOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        httpContext.Response.Cookies.Append("access_token", "", expiredOptions);
        httpContext.Response.Cookies.Append("refresh_token", "", expiredOptions);
    }
}
