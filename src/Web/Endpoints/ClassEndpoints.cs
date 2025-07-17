using CleanArchitectureBase.Application.Classes.Lecturer;
using CleanArchitectureBase.Application.Classes.Student;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Entities;
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
            .MapPost(InviteStudent, "/{ClassId}/Invitations")
            .MapPost(AddQuestionSet, "/{ClassId}/Questionsets/{QuestionSetId}")
            .MapPost(CreateTest, "/Test")
            .MapGet(SearchStudent, "/{ClassId}/Students")
            .MapGet(SearchClass, "")
            .MapGet(SearchTest, "/{ClassId}/Tests")
            .MapGet(GetTestSchedule, "/{ClassId}/Schedule")
            .MapGet(GetClassById, "/{ClassId}")
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
        var result = await sender.Send(new GetClassById{ClassId = classId});
        return result.ToOk();
    } 
    
    public async Task<Ok<ApiResponse<ClassCodeDto>>> InviteStudent(
        [FromRoute] Guid classId,
        [FromQuery] double expiredTime,
        ISender sender)
    {
        var rq = new InviteStudentCommand
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
        var rq = new SearchStudentInClass
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
    
    public async Task<Ok<ApiResponse<PaginatedList<TestSearchResultDto>>>> SearchTest(
        [FromQuery] string? status,
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
    
    public async Task<Ok<ApiResponse<PaginatedList<ClassSearchResultDto>>>> SearchClass(
        [FromQuery] string? shareMode,
        [FromQuery] string? name,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchClass()
        {
           ShareMode = shareMode,
           Name = name,
           PageNumber = pageNumber,
           PageSize = pageSize,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<List<TestScheduleResponse>>>> GetTestSchedule(
        [FromRoute] Guid classId,
        [FromQuery] int? month,
        [FromQuery] int? year,
        ISender sender)
    {
        var rq = new GetTestSchedule { ClassId = classId, Month = month, Year = year };
        var result = await sender.Send(rq);
        return result.ToOk();
        
    }

    public async Task<Ok<ApiResponse<UpdatePositionDto>>> UpdatePosition(
        [FromRoute] Guid classId,
        [FromRoute] Guid userId,
        [FromQuery] ClassShareMode position,
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
        var rq = new RemoveQuestionSetCommand()
        {
            ClassId = classId,
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> CreateTest([FromBody] CreateTestCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
