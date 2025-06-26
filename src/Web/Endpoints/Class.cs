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
            .MapPost(JoinClassByCode, "students")
            .MapPost(InviteStudent, "/{ClassId}/invitations")
            .MapGet(SearchStudent, "/{ClassId}/students")
            .MapGet(SearchClass, "/classes")
            .MapPatch("/{ClassId}/members/{UserId}", UpdatePosition);
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
    
    public async Task<Ok<ApiResponse<ClassCodeDto>>> InviteStudent(
        [FromRoute] Guid ClassId,
        [FromQuery] double ExpiredTime,
        ISender sender)
    {
        var rq = new InviteStudentCommand
        {
            ClassId = ClassId,
            ExpiredTime = ExpiredTime,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<StudentSearchResultDto>>>> SearchStudent(
        [FromRoute] Guid ClassId,
        [FromQuery] string? Keyword,
        [FromQuery] string? FieldName,
        ISender sender,
        [FromQuery] int PageNumber = 1,
        [FromQuery] int PageSize = 5)
    {
        var rq = new SearchStudent
        {
            ClassId = ClassId,
            Keyword = Keyword,
            FieldName = FieldName,
            PageNumber = PageNumber,
            PageSize = PageSize,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<ClassSearchResultDto>>>> SearchClass(
        [FromQuery] ClassShareMode? shareMode,
        [FromQuery] string? Name,
        ISender sender,
        [FromQuery] int PageNumber = 1,
        [FromQuery] int PageSize = 5)
    {
        var rq = new SearchClass()
        {
           ShareMode = shareMode,
           Name = Name,
           PageNumber = PageNumber,
           PageSize = PageSize,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<UpdatePositionDto>>> UpdatePosition(
        [FromRoute] Guid ClassId,
        [FromRoute] Guid userId,
        [FromQuery] ClassShareMode Position,
        ISender sender)
    {
        var rq = new UpdatePositionCommand()
        {
            ClassId = ClassId,
            Position = Position,
            UserId = userId
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }

}
