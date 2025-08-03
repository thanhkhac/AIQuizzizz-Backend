using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Application.FolderTest.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Folder : EndpointGroupBase
{
    public override void Map(WebApplication app){
        app.MapGroup(this)
            .MapGet(SearchFolderTest, "")
            .MapGet(SearchTestTemplateInFolder, "/{FolderId}/TestTemplates")
            .MapGet(GetSharingInFolder, "/{FolderId}/Sharing")
            .MapPost(AddTestTemplateToFolder, "/{FolderId}/TestTemplate/{TestTemplateId}")
            .MapPost(AddSharingInFolder, "/{FolderId}/Sharing")
            .MapPost(CreateFolder, "")
            .MapDelete(DeleteFolder, "/{FolderId}/")
            .MapDelete(RemoveTestTestTemplateInFolder, "/{FolderId}/TestTemplate/{TestTemplateId}")
            .MapPatch(UpdateSharingInFolder, "/{FolderId}/Sharing");
        
        app.MapGroup(this)
            .MapPatch("/{FolderId}", UpdateFolder);
}

    public async Task<Ok<ApiResponse<Guid>>> CreateFolder([FromBody] CreateFolderCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> AddTestTemplateToFolder(
        [FromRoute] Guid folderId,
        [FromRoute] Guid testTemplateId,
        ISender sender)
    {
        var rq = new AddTestTemplateToFolderCommand { FolderId = folderId, TestTemplateId = testTemplateId, };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> RemoveTestTestTemplateInFolder(
        [FromRoute] Guid folderId,
        [FromRoute] Guid testTemplateId,
        ISender sender)
    {
        var rq = new RemoveTestTemplateInFolderCommand { FolderId = folderId, TestTemplateId = testTemplateId, };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> AddSharingInFolder(
        [FromRoute] Guid folderId,
        [FromBody] AddSharingInFolderCommand rq,
        ISender sender)
    {
        rq.FolderId = folderId;
        var result = await sender.Send(rq);
        return result.ToOk();
    } 
    
    public async Task<Ok<ApiResponse<PaginatedList<SearchFolderTestDto>>>> SearchFolderTest(
        [FromBody] SearchFolderQuery rq,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var result = await sender.Send(rq);
        
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<TestTemplateDto>>>> SearchTestTemplateInFolder(
        [FromRoute] Guid folderId,
        [FromBody] SearchTestTemplateInFolderQuery rq,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        rq.FolderId = folderId;
        rq.PageNumber = pageNumber;
        rq.PageSize = pageSize;
        
        var result = await sender.Send(rq);
        
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<ResourceShareDto>>> GetSharingInFolder([FromRoute] Guid folderId, ISender sender)
    {
        var result = await sender.Send(new GetSharingInFolderQuery{FolderId = folderId});
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> DeleteFolder([FromRoute] Guid folderId, ISender sender)
    {
        var result = await sender.Send(new DeleteFolderCommand{FolderId = folderId});
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> UpdateSharingInFolder(
        [FromRoute] Guid folderId,
        [FromBody] UpdateSharingInFolderCommand rq,
        ISender sender)
    {
        rq.FolderId = folderId;
        var result = await sender.Send(rq);
        return result.ToOk();
    } 
    
    public async Task<Ok<ApiResponse<Guid>>> UpdateFolder(
        [FromRoute] Guid folderId,
        [FromBody] UpdateFolderCommand rq,
        ISender sender)
    {
        rq.FolderId = folderId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
