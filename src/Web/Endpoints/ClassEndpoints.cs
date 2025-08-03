using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Classes.Dto;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
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
        
        app.MapGroup(this)
            .MapPatch("/{ClassId}", UpdateClass);
    }

    /// <summary>
    /// User - create new class
    /// </summary>
    /// <param name="rq"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<Guid>>> CreateClass([FromBody] CreateClassCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    /// <summary>
    /// User - join class by code
    /// </summary>
    /// <param name="rq"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<Unit>>> JoinClassByCode([FromBody] JoinClassByCodeCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    /// <summary>
    /// Lecture/Student - Get class detail data
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<ClassDetailDto>>> GetClassById([FromRoute] Guid classId, ISender sender)
    {
        var result = await sender.Send(new GetClassByIdQuery{ClassId = classId});
        return result.ToOk();
    } 
    
    /// <summary>
    /// Retrieves the test schedule for a specified class, optionally filtered by month and year
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="month"></param>
    /// <param name="year"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Lecturer - Get class invitation code for view
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<string?>>> GetInviteStudentCode(
        [FromRoute] Guid classId,
        ISender sender)
    {
        var result = await sender.Send(new GetInviteStudentCodeQuery{ClassId = classId});
        return result.ToOk();
    } 
    
    /// <summary>
    /// Lecturer - Create new invitation code
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="rq"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<ClassCodeDto>>> CreateInviteCode(
        [FromRoute] Guid classId,
        [FromBody] CreateInviteCodeCommand rq,
        ISender sender)
    {
        rq.ClassId = classId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    /// <summary>
    /// Lecturer/Student search student in class
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="keyword"></param>
    /// <param name="fieldName"></param>
    /// <param name="sender"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Search test in class
    /// </summary>
    /// <param name="status"></param>
    /// <param name="testName"></param>
    /// <param name="classId"></param>
    /// <param name="sender"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Lecturer - Search added question sets in class
    /// </summary>
    /// <param name="name"></param>
    /// <param name="classId"></param>
    /// <param name="sender"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<PaginatedList<QuestionSetForListResponseDto>>>> SearchQuestionSet(
        [FromQuery] string? name,
        [FromRoute] Guid classId,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchQuestionSetQuery()
        {
            ClassId = classId,
            Name = name,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    /// <summary>
    /// User - Search class the current user has joined
    /// </summary>
    /// <param name="shareMode"></param>
    /// <param name="name"></param>
    /// <param name="sender"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Class onwer - Update position of members in class
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="userId"></param>
    /// <param name="rq"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<UpdatePositionDto>>> UpdatePosition(
        [FromRoute] Guid classId,
        [FromRoute] Guid userId,
        [FromBody] UpdatePositionCommand rq,
        ISender sender)
    {
        rq.ClassId = classId;
        rq.UserId = userId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    /// <summary>
    /// Class onwer - Update class
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="rq"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<Guid>>> UpdateClass(
        [FromRoute] Guid classId,
        [FromBody] UpdateClassCommand rq,
        ISender sender)
    {
        rq.ClassId = classId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    /// <summary>
    /// Class owner - Delete class
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
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
