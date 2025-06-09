using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Account : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        // app.MapGroup(this)
        //     .MapIdentityApi<ApplicationUser>();
        app.MapGroup(this)
            .MapPost(RegisterUser, "Register");
    }
    
    
    public async Task<Ok<ApiResponse<string>>> RegisterUser([FromBody] RegisterUserCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();    
    }
    
}
