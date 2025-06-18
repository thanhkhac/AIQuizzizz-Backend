using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Account : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)    
            .MapPost(RegisterUser, "Register")
            .MapPost(Login, "Login")
            .MapGet(GetProfile, "Profile")
            ;
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

}
