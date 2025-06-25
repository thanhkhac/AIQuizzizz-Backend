using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tests.LecturerTests;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Test : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(SearchTest, "/{ClassId}/search-test");
    }

    public async Task<Ok<ApiResponse<List<TestSearchResultDto>>>> SearchTest(
        [FromQuery] TestStatus? status,
        [FromQuery] string? TestName,
        [FromRoute] Guid ClassId,
        ISender sender)
    {
        var rq = new SearchTest()
        {
            ClassId = ClassId,
            TestName = TestName,
            Status = status
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
