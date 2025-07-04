using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class AccountEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
    
        app.MapGroup(this)
            .MapGet(GetProfile, "Profile");
    }
    
   
    public async Task<Ok<ApiResponse<UserProfileDto>>> GetProfile(ISender sender, HttpContext httpContext)
    {
        var result = await sender.Send(new GetProfileQuery());
        return result.ToOk();
    }
}
