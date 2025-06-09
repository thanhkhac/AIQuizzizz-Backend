
using CleanArchitectureBase.Application.Security.Queries;
using CleanArchitectureBase.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;


public class Roles : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        // app.MapGroup(this)
        //     .WithName("Roles")
        //     .MapGet(GetPermissionGroups)
        //     // .MapPost(RegisterUser)
        //     ;
    }

    public async Task<List<PermissionGroupDto>> GetPermissionGroups(ISender sender)
    {
        return await sender.Send(new GetPermissionGroupsQuery());
    }
}
