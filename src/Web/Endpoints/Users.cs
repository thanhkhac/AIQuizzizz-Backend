using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.Users.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Users : EndpointGroupBase
{
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
            .MapPatch("{UserId}/Role", ChangeRole);
        
        app.MapGroup(this)
            .MapPatch("/{UserId}/Active", ActiveUser);
        
        app.MapGroup(this)
            .MapPatch("/{UserId}/Ban", BanUser);

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
        var token = httpContext.Request.Headers["Authorization"].ToString();
        Console.WriteLine($"Token received: {token}");
        
        if (httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated)
        {
            Console.WriteLine("User authenticated: " + httpContext.User.Identity.Name);
            foreach (var claim in httpContext.User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }
        }
        else
        {
            Console.WriteLine("User not authenticated");
        }
        
        var result = await sender.Send(new GetProfileQuery());
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<TokenDto>>> RefreshToken([FromBody] RefreshTokenCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse>> RevokeToken([FromBody] RevokeTokenCommand command, ISender sender)
    {
        await sender.Send(command);
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
    
    public async Task<Ok<ApiResponse<Guid>>> ChangeRole(
        [FromRoute] Guid userId,
        [FromQuery] string role,
        ISender sender)
    {
        var rq = new ChangeAccountRoleCommand()
        {
            UserId = userId,
            Role = role,
        };
        
        var result = await sender.Send(rq);
        return result.ToOk();
    }

}
