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

    public async Task<Ok<ApiResponse<PaginatedList<TestSearchResultDto>>>> SearchTest(
        [FromQuery] TestStatus? Status,
        [FromQuery] string? TestName,
        [FromRoute] Guid ClassId,
        ISender sender,
        [FromQuery] int PageNumber = 1,
        [FromQuery] int PageSize = 5)
    {
        var rq = new SearchTest()
        {
            ClassId = ClassId,
            TestName = TestName,
            Status = Status,
            PageNumber = PageNumber,
            PageSize = PageSize,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
