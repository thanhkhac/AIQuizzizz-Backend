using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Application.TestTemplates.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class TestTemplate : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(SearchTestTemplate, "")
            .MapGet(GetTestTemplatePermissions, "/{testTemplateId}/Permissions")
            .MapGet(GetTestTemplateDetail, "/{testTemplateId}")
            .MapGet(GetSharingInTestTemplate, "/{testTemplateId}/Sharing")
            .MapPost(AddSharingInTestTemplate, "/{testTemplateId}/Sharing")
            .MapPost(CreateTestTemplate, "")
            .MapDelete(DeleteTestTemplate, "/{testTemplateId}")
            .MapPatch("/{TestTemplateId}", UpdateTestTemplate);

        app.MapGroup(this).DisableAntiforgery()
            .MapPost(ImportFileTestTemplate, "/ImportFile")
            .MapPatch(UpdateSharingInTestTemplate, "/{testTemplateId}/Sharing");
        ;
    }

    public async Task<Ok<ApiResponse<PaginatedList<TestTemplateDto>>>> SearchTestTemplate(
        [FromQuery] string? name,
        [FromQuery] string? sharedMode,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchTestTemplateQuery
        {
            TestTemplateName = name, SharedMode = sharedMode, PageNumber = pageNumber, PageSize = pageSize,
        };
        
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> AddSharingInTestTemplate(
        [FromRoute] Guid testTemplateId,
        [FromBody] AddSharingTestTemplateCommand rq,
        ISender sender)
    {
        rq.TestTemplateId = testTemplateId;
        var result = await sender.Send(rq);
        return result.ToOk();
    } 
    
    public async Task<Ok<ApiResponse<Guid>>> UpdateSharingInTestTemplate(
        [FromRoute] Guid testTemplateId,
        [FromBody] UpdateSharingInTestTemplateCommand rq,
        ISender sender)
    {
        rq.TestTemplateId = testTemplateId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<ResourceShareDto>>> GetSharingInTestTemplate([FromRoute] Guid testTemplateId, ISender sender)
    {
        var result = await sender.Send(new GetSharingInTestTemplateQuery{TestTemplateId = testTemplateId});
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> CreateTestTemplate([FromBody] CreateTestTemplateCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<TestTemplatePermissionsDto>>> GetTestTemplatePermissions([FromRoute] Guid testTemplateId, ISender sender)
    {
        var query = new GetTestTemplatePermissionsQuery
        {
            TestTemplateId = testTemplateId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<TestTemplateDetailDto>>> GetTestTemplateDetail([FromRoute] Guid testTemplateId, ISender sender)
    {
        var query = new GetTestTemplateDetailQuery
        {
            TestTemplateId = testTemplateId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<ImportedQuestionDto>>> ImportFileTestTemplate(
        [FromForm] IFormFile file,
        ISender sender)
    {
        var rq = new FileStreamData()
        {
            Data = file.OpenReadStream(),
            ContentType = file.ContentType,
            FileName = file.FileName,
        };
        var result = await sender.Send(new ImportFileTestTemplateCommand
        {
            FileData = rq
        });
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> UpdateTestTemplate(
        [FromRoute] Guid testTemplateId,
        [FromBody] UpdateTestTemplateCommand rq,
        ISender sender)
    {
        rq.TestTemplateId = testTemplateId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> DeleteTestTemplate(
        [FromRoute] Guid testTemplateId,
        ISender sender)
    {
        var result = await sender.Send(new DeleteTestTemplateCommand{TestTemplateId = testTemplateId});
        return result.ToOk();
    } 
}
