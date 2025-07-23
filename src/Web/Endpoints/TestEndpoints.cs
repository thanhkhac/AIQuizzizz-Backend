using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tests;
using CleanArchitectureBase.Application.Tests.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using HistoryTestDto = CleanArchitectureBase.Application.Tests.Dto.HistoryTestDto;

namespace CleanArchitectureBase.Web.Endpoints;

public class Test : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetTestDetail, "/{TestId}")
            .MapGet(GetTestResultOfClass, "/{TestId}/Class/Result")
            .MapGet(GetHistoryTest, "/{TestId}/History")
            .MapPost(CreateTest, "")
            .MapPost(SubmitTestAttempt, "/Submit")
            .MapPost(StartAttemptTestTestAttempt, "/{TestId}/Attempt")
            .MapDelete(DeleteTest, "/{TestId}/");

        app.MapGroup(this)
            .MapPatch("/{TestId}", EditTest);
    }
    
    public async Task<Ok<ApiResponse<TestDetailDto>>> GetTestDetail([FromRoute] Guid testId, ISender sender)
    {
        var rq = new GetTestDetailQuery { TestId = testId };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<ResultTestOfClassDto>>>> GetTestResultOfClass(
        [FromRoute] Guid testId,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new GetTestResultOfClassQuery { TestId = testId, PageNumber = pageNumber, PageSize = pageSize };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> CreateTest([FromBody] CreateTestCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<HistoryTestDto>>>> GetHistoryTest(
        [FromRoute] Guid testId,
        [FromQuery] Guid? userId,
        [FromQuery] bool? isPassed,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var query = new GetUserTestHistoryQuery
        {
            TestId = testId,
            IsPassed = isPassed,
            UserId = userId,
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
    
    public async Task<Ok<ApiResponse<Guid>>> DeleteTest(
        [FromRoute] Guid testId,
        ISender sender)
    {
        var result = await sender.Send(new DeleteTestCommand{TestId = testId});
        return result.ToOk();
    } 
    
    public async Task<Ok<ApiResponse<Guid>>> EditTest(
        [FromRoute] Guid testId,
        [FromBody] EditTestCommand rq,
        ISender sender)
    {
        rq.TestId = testId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
