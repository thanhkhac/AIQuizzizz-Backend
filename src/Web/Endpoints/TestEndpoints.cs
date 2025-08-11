using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tests;
using CleanArchitectureBase.Application.Tests.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using HistoryTestDto = CleanArchitectureBase.Application.Tests.Dto.HistoryTestDto;
using TestResultDto = CleanArchitectureBase.Application.Tests.TestResultDto;

namespace CleanArchitectureBase.Web.Endpoints;

public class Test : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapGet(GetTestDetail, "/{TestId}");
        group.MapGet(GetTestResultOfClass, "/{TestId}/Class/Result");
        group.MapGet(GetHistoryTest, "/{TestId}/History");
        group.MapGet(GetTestSchedule, "/Schedule");
        group.MapGet(GetReviewTest, "/{AttemptId}/Review");


        group.MapPost(CreateTest, "");
        group.MapPost(SubmitTestAttempt, "/Submit");
        group.MapPost(StartAttemptTestTestAttempt, "/{TestId}/Attempt");

        group.MapDelete(DeleteTest, "/{TestId}/");
        group.MapPatch(EditTest, "/{TestId}");
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
    
    public async Task<Ok<ApiResponse<ReviewTestDto>>> GetReviewTest(
        [FromRoute] Guid attemptId,
        ISender sender
        )
    {
        var rq = new GetReviewTestQuery { AttemptId = attemptId};
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
        [FromBody] UpdateTestCommand rq,
        ISender sender)
    {
        rq.TestId = testId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    /// <summary>
    /// Retrieves the test schedule for a specified class, optionally filtered by month and year
    /// </summary>
    /// <param name="month"></param>
    /// <param name="year"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<List<TestScheduleResponse>>>> GetTestSchedule(
        [FromQuery] int? month,
        [FromQuery] int? year,
        ISender sender)
    {
        var rq = new GetTestScheduleQuery() { Month = month, Year = year };
        var result = await sender.Send(rq);
        return result.ToOk();
        
    }
}
