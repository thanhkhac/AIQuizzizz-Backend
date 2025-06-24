using CleanArchitectureBase.Application.Class;
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
            .MapPost(JoinClassByCode, "join")
            .MapPost(InviteStudent, "invite")
            .MapGet(SearchStudent, "/{ClassId}/search-student");
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
    
    public async Task<Ok<ApiResponse<ClassCodeDto>>> InviteStudent([FromBody] InviteStudentCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<List<StudentDetailDto>>>> SearchStudent(
        [FromQuery] string keyword,
        [FromQuery] string fieldName,
        [FromRoute] Guid ClassId,
        ISender sender)
    {
        var rq = new SearchStudent
        {
            ClassId = ClassId,
            Keyword = keyword,
            FieldName = fieldName
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }

}
