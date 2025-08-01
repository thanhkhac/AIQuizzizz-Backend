using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.Users.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Web.Endpoints;

public class AuthenticationEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(RegisterUser, "Register");
        group.MapPost(Login, "Login");
        group.MapPost(GoogleLogin, "GoogleLogin");
        group.MapPost(RefreshToken, "RefreshToken");
        group.MapPost(RevokeToken, "RevokeToken");
        group.MapPost(LogOut, "LogOut");

        group.MapPost(RequestEmailVerification, "RequestEmailVerification");
        group.MapPost(VerifyEmail, "VerifyEmail");

        group.MapPost(RequestPasswordReset, "RequestPasswordReset");
        group.MapPost(ResetPassword, "ResetPassword");

        group.MapPost(ChangePassword, "ChangePassword");
        group.MapPost(SetPassword, "SetPassword");
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

    public async Task<Ok<ApiResponse>> RequestEmailVerification([FromBody] RequestEmailVerificationCommand command, ISender sender)
    {
        await sender.Send(command);
        return ApiResponse.SuccessResult().ToOk();
    }
    
    public async Task<Ok<ApiResponse>> VerifyEmail([FromBody] VerifyEmailCommand command, ISender sender)
    {
        await sender.Send(command);
        return ApiResponse.SuccessResult().ToOk();
    }

    public async Task<Ok<ApiResponse>> RequestPasswordReset([FromBody] RequestPasswordResetCommand command, ISender sender)
    {
        await sender.Send(command);
        return ApiResponse.SuccessResult().ToOk();
    }

    public async Task<Ok<ApiResponse>> ResetPassword([FromBody] ResetPasswordCommand command, ISender sender)
    {
        await sender.Send(command);
        return ApiResponse.SuccessResult().ToOk();
    }

    public async Task<Ok<ApiResponse>> ChangePassword([FromBody] ChangePasswordCommand command, ISender sender)
    {
        await sender.Send(command);
        return ApiResponse.SuccessResult().ToOk();
    }
    
    public async Task<Ok<ApiResponse>> SetPassword([FromBody] SetPasswordCommand command, ISender sender)
    {
        await sender.Send(command);
        return ApiResponse.SuccessResult().ToOk();
    }
}
