using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tests;
using CleanArchitectureBase.Application.Tests.Lecturer;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Test : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(SearchTest, "/{ClassId}/Tests")
            .MapPost(CreateTestTemplate, "/Templates");
    }

    public async Task<Ok<ApiResponse<PaginatedList<TestSearchResultDto>>>> SearchTest(
        [FromQuery] TestStatus? status,
        [FromQuery] string? testName,
        [FromRoute] Guid classId,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchTestInClass()
        {
            ClassId = classId,
            TestName = testName,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> CreateTestTemplate([FromBody] CreateTestTemplateCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
