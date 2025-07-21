using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Classes.Dto;
using CleanArchitectureBase.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Class : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(CreateClass, "")
            .MapPost(JoinClassByCode, "Students")
            .MapPost(CreateInviteCode, "/{ClassId}/Invitations")
            .MapPost(AddQuestionSet, "/{ClassId}/Questionsets/{QuestionSetId}")
            .MapGet(SearchStudent, "/{ClassId}/Students")
            .MapGet(SearchClass, "")
            .MapGet(SearchTest, "/{ClassId}/Tests")
            .MapGet(GetInviteStudentCode, "/{ClassId}/Invitation-Code")
            .MapGet(SearchQuestionSet, "/{ClassId}/Questionsets")
            .MapGet(GetClassById, "/{ClassId}")
            .MapGet(GetTestSchedule, "/{ClassId}/Schedule")
            .MapDelete(DeleteClass, "/{ClassId}")
            .MapDelete(RemoveStudent, "/{ClassId}/Members/{UserId}")
            .MapDelete(RemoveQuestionSet, "/{ClassId}/Questionsets/{QuestionSetId}")
            .MapPatch("/{ClassId}/Members/{UserId}", UpdatePosition);
    }

    public async Task<Ok<ApiResponse<Guid>>> CreateClass([FromBody] CreateClassCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Unit>>> JoinClassByCode([FromBody] JoinClassByCodeCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<ClassDetailDto>>> GetClassById([FromRoute] Guid classId, ISender sender)
    {
        var result = await sender.Send(new GetClassByIdQuery{ClassId = classId});
        return result.ToOk();
    } 
    
    public async Task<Ok<ApiResponse<List<TestScheduleResponse>>>> GetTestSchedule(
        [FromRoute] Guid classId,
        [FromQuery] int? month,
        [FromQuery] int? year,
        ISender sender)
    {
        var rq = new GetTestScheduleQuery() { ClassId = classId, Month = month, Year = year };
        var result = await sender.Send(rq);
        return result.ToOk();
        
    }
    
    public async Task<Ok<ApiResponse<string?>>> GetInviteStudentCode([FromRoute] Guid classId, ISender sender)
    {
        var result = await sender.Send(new GetInviteStudentCodeQuery{ClassId = classId});
        return result.ToOk();
    } 
    
    public async Task<Ok<ApiResponse<ClassCodeDto>>> CreateInviteCode(
        [FromRoute] Guid classId,
        [FromQuery] double expiredTime,
        ISender sender)
    {
        var rq = new CreateInviteCodeCommand
        {
            ClassId = classId,
            ExpiredTime = expiredTime,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<StudentSearchResultDto>>>> SearchStudent(
        [FromRoute] Guid classId,
        [FromQuery] string? keyword,
        [FromQuery] string? fieldName,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchStudentInClassQuery
        {
            ClassId = classId,
            Keyword = keyword,
            FieldName = fieldName,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<SearchTestResultDto>>>> SearchTest(
        [FromQuery] string? status,
        [FromQuery] string? testName,
        [FromRoute] Guid classId,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchTestInClassQuery()
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
    
    public async Task<Ok<ApiResponse<PaginatedList<SearchQuestionSetDto>>>> SearchQuestionSet(
        [FromQuery] string? name,
        [FromQuery] string? shareMode,
        [FromRoute] Guid classId,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchQuestionSetQuery()
        {
            ClassId = classId,
            Name = name,
            ShareMode = shareMode,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<ClassSearchResultDto>>>> SearchClass(
        [FromQuery] string? shareMode,
        [FromQuery] string? name,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchClassQuery()
        {
           ShareMode = shareMode,
           Name = name,
           PageNumber = pageNumber,
           PageSize = pageSize,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<UpdatePositionDto>>> UpdatePosition(
        [FromRoute] Guid classId,
        [FromRoute] Guid userId,
        [FromQuery] string position,
        ISender sender)
    {
        var rq = new UpdatePositionCommand()
        {
            ClassId = classId,
            Position = position,
            UserId = userId
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> DeleteClass(
        [FromRoute] Guid classId,
        ISender sender)
    {
        var rq = new DeleteClassCommand()
        {
            ClassId = classId
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> RemoveStudent(
        [FromRoute] Guid classId,
        [FromRoute] Guid userId,
        ISender sender)
    {
        var rq = new RemoveStudentCommand()
        {
            ClassId = classId,
            UserId = userId
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> AddQuestionSet(
        [FromRoute] Guid classId,
        [FromRoute] Guid questionSetId,
        ISender sender)
    {
        var rq = new AddQuestionSetCommand()
        {
            ClassId = classId,
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> RemoveQuestionSet(
        [FromRoute] Guid classId,
        [FromRoute] Guid questionSetId,
        ISender sender)
    {
        var rq = new RemoveQuestionSetFromClassCommand()
        {
            ClassId = classId,
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
