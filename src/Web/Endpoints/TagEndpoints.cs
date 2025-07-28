using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tags;
using CleanArchitectureBase.Application.Tags.Dto;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitectureBase.Web.Endpoints;

public class TagEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(SearchTags);
    }

    public async Task<Ok<ApiResponse<List<TagForListReponseDto>>>> SearchTags(
        [AsParameters] SearchTagQuery request,
        ISender sender)
    {
        var result = await sender.Send(request);
        return result.ToOk();
    }
}
