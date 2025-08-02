using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Application.FolderTest.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Folder : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapGet(SearchFolderTest, "");
        group.MapGet(SearchTestTemplateInFolder, "/{FolderId}/TestTemplates");
        group.MapGet(GetSharingInFolder, "/{FolderId}/Sharing");
        group.MapPost(AddTestTemplateToFolder, "/{FolderId}/TestTemplate/{TestTemplateId}");
        group.MapPost(CreateFolder, "");
        group.MapDelete(DeleteFolder, "/{FolderId}/");
        group.MapDelete(RemoveTestTestTemplateInFolder, "/{FolderId}/TestTemplate/{TestTemplateId}");
        group.MapPatch(UpdateSharingInFolder, "/{FolderId}/Sharing");
        group.MapPatch(UpdateFolder, "/{FolderId}");
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
        var rq = new AddTestTemplateToFolderCommand
        {
            FolderId = folderId,
            TestTemplateId = testTemplateId,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> RemoveTestTestTemplateInFolder(
        [FromRoute] Guid folderId,
        [FromRoute] Guid testTemplateId,
        ISender sender)
    {
        var rq = new RemoveTestTemplateInFolderCommand
        {
            FolderId = folderId,
            TestTemplateId = testTemplateId,
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }


    public async Task<Ok<ApiResponse<PaginatedList<SearchFolderTestDto>>>> SearchFolderTest(
        [FromQuery] string? folderName,
        [FromQuery] string? shareMode,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchFolderQuery
        {
            ShareMode = shareMode,
            FolderName = folderName,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        var result = await sender.Send(rq);

        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<PaginatedList<TestTemplateDto>>>> SearchTestTemplateInFolder(
        [FromRoute] Guid folderId,
        [FromQuery] string? testTemplateName,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchTestTemplateInFolderQuery
        {
            FolderId = folderId,
            TestTemplateName = testTemplateName,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        var result = await sender.Send(rq);

        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<ResourceShareDto>>> GetSharingInFolder([FromRoute] Guid folderId, ISender sender)
    {
        var result = await sender.Send(new GetSharingInFolderQuery
        {
            FolderId = folderId
        });
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> DeleteFolder([FromRoute] Guid folderId, ISender sender)
    {
        var result = await sender.Send(new DeleteFolderCommand
        {
            FolderId = folderId
        });
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
        [FromQuery] string name,
        ISender sender)
    {
        var rq = new UpdateFolderCommand
        {
            FolderId = folderId,
            Name = name
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
