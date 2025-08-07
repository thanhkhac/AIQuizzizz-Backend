using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.zPlayGround;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitectureBase.Web.Endpoints.zPlayGround;

public class PlayGroundEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);
        group.MapGet(Test, "Test");
    }

    public async Task<Ok<ApiResponse<string>>> Test(ISender sender)
    {
        var result = await sender.Send(new ZPlayGroundQuery()
        {
        });
        return result.ToOk();
    }
}
