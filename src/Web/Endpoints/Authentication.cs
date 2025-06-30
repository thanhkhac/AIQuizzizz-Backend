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
            .MapPost(RegisterUser, "Register")
            .MapPost(Login, "Login")
            .MapPost(GoogleLogin, "GoogleLogin")
            .MapPost(GoogleRegister, "GoogleRegister")
            ;
    }

    public async Task<Ok<ApiResponse<string>>> RegisterUser([FromBody] RegisterUserCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<TokenDto>>> Login([FromBody] LoginCommand command, ISender sender, HttpContext httpContext,
        IOptions<JwtSettings> jwtSettings)
    {
        var result = await sender.Send(command);

        SetTokenCookies(httpContext, result.AccessToken, result.RefreshToken, jwtSettings.Value);

        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<TokenDto>>> GoogleLogin([FromBody] GoogleLoginCommand command, ISender sender, HttpContext httpContext,
        IOptions<JwtSettings> jwtSettings)
    {
        var result = await sender.Send(command);

        SetTokenCookies(httpContext, result.AccessToken, result.RefreshToken, jwtSettings.Value);

        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<TokenDto>>> GoogleRegister([FromBody] GoogleRegisterCommand command, ISender sender,
        HttpContext httpContext,
        IOptions<JwtSettings> jwtSettings)
    {
        var result = await sender.Send(command);
        SetTokenCookies(httpContext, result.AccessToken, result.RefreshToken, jwtSettings.Value);
        return result.ToOk();
    }


    public async Task<Ok<ApiResponse<TokenDto>>> RefreshToken([FromBody] RefreshTokenCommand command,
        ISender sender,
        HttpContext httpContext,
        IOptions<JwtSettings> jwtSettings, [FromQuery] bool useCookies = true)
    {
        if (string.IsNullOrEmpty(command.AccessToken))
        {
            command.AccessToken = httpContext.Request.Cookies["access_token"];
        }
        if (string.IsNullOrEmpty(command.RefreshToken))
        {
            command.RefreshToken = httpContext.Request.Cookies["refresh_token"];
        }

        var result = await sender.Send(command);


        SetTokenCookies(httpContext, result.AccessToken, result.RefreshToken, jwtSettings.Value);

        return result.ToOk();
    }

    public async Task<Ok<ApiResponse>> RevokeToken([FromBody] RevokeTokenCommand command, ISender sender, HttpContext httpContext)
    {
        if (string.IsNullOrEmpty(command.RefreshToken))
        {
            command.RefreshToken = httpContext.Request.Cookies["refresh_token"];
        }
        await sender.Send(command);

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
