using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Infrastructure.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Web.Endpoints;

public class Users : EndpointGroupBase
{
    private readonly JwtSettings _jwtSettings;
    public Users(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    //TODO: Tách endpoint user ra chỗ khác
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(RegisterUser, "register")
            .MapPost(Login, "login")
            .MapGet(GetProfile, "profile")
            .MapPost(RefreshToken, "RefreshToken")
            .MapPost(RevokeToken, "RevokeToken")
            .MapGet(GetAllAccount, "")
            .MapPost(BanUser, "/{UserId}/Ban")
            .MapPatch("/{UserId}/Active", ActiveUser);
    }


    public async Task<Ok<ApiResponse<string>>> RegisterUser([FromBody] RegisterUserCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<TokenDto>>> Login([FromBody] LoginCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }


    public async Task<Ok<ApiResponse<UserProfileDto>>> GetProfile(ISender sender, HttpContext httpContext)
    {
        var result = await sender.Send(new GetProfileQuery());
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<TokenDto>>> RefreshToken([FromBody] RefreshTokenCommand command, ISender sender, HttpContext httpContext)
    {
        command.AccessToken ??= httpContext.Request.Cookies["access_token"];
        command.RefreshToken ??= httpContext.Request.Cookies["refresh_token"];

        var result = await sender.Send(command);
        
        //Set token vào cookie
        SetTokenCookies(httpContext, result.AccessToken, result.RefreshToken);
        
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

    public async Task<Ok<ApiResponse<PaginatedList<AccountDto>>>> GetAllAccount(
        ISender sender,
        [FromQuery] string? keyword,
        [FromQuery] string? fieldName,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new GetAllAccountCommand
        {
            Keyword = keyword,
            FieldName = fieldName,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> BanUser([FromRoute] Guid userId, ISender sender)
    {
        var rq = new BanAccountCommand()
        {
            UserId = userId,
        };

        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> ActiveUser([FromRoute] Guid userId, ISender sender)
    {
        var rq = new ActiveAccountCommand()
        {
            UserId = userId,
        };

        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    private void SetTokenCookies(HttpContext httpContext, string accessToken, string refreshToken)
    {
        var expiredOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        };
        
        httpContext.Response.Cookies.Append("access_token", accessToken, expiredOptions);
        httpContext.Response.Cookies.Append("refresh_token", refreshToken, expiredOptions);
    }


    private void ClearTokenCookies(HttpContext httpContext)
    {
        var expiredOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        httpContext.Response.Cookies.Append("access_token", "", expiredOptions);
        httpContext.Response.Cookies.Append("refresh_token", "", expiredOptions);
    }

}
