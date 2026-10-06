using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.Users.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

    // Token trả trong body; client gửi lại qua header "Authorization: Bearer <accessToken>"
    public async Task<Ok<ApiResponse<TokenDto>>> Login([FromBody] LoginCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<TokenDto>>> GoogleLogin([FromBody] GoogleLoginCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    // Body: { accessToken, refreshToken }
    public async Task<Ok<ApiResponse<TokenDto>>> RefreshToken([FromBody] RefreshTokenCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    // Body: { refreshToken }
    public async Task<Ok<ApiResponse>> RevokeToken([FromBody] RevokeTokenCommand command, ISender sender)
    {
        await sender.Send(command);
        return ApiResponse.SuccessResult().ToOk();
    }

    public Ok<ApiResponse> LogOut()
    {
        return ApiResponse.SuccessResult().ToOk();
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
