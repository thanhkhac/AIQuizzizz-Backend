using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tests;
using CleanArchitectureBase.Application.Tests.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Test : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetHistoryTest, "/{TestId}/History")
            .MapPost(CreateTest, "")
            .MapPost(SubmitTestAttempt, "/Submit")
            .MapPost(StartAttemptTestTestAttempt, "/{TestId}/Attempt");
    }
    
    public async Task<Ok<ApiResponse<Guid>>> CreateTest([FromBody] CreateTestCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<HistoryTestDto>>>> GetHistoryTest(
        [FromRoute] Guid testId,
        [FromQuery] string? studentName,
        [FromQuery] bool? isPassed,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var query = new GetUserTestHistoryCommand
        {
            TestId = testId,
            IsPassed = isPassed,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<AttemptDetailDto>>> StartAttemptTestTestAttempt(
        [FromRoute] Guid testId,
        ISender sender)
    {
        var result = await sender.Send(new StartAttemptTestCommand{TestId = testId});
        return result.ToOk();
    } 

    public async Task<Ok<ApiResponse<TestResultDto>>> SubmitTestAttempt(
        [FromBody] SubmitTestAttemptCommand userAnswer,
        ISender sender)
    {
        var result = await sender.Send(userAnswer);
        return result.ToOk();
    }
}
