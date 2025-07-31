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
            var group = app.MapGroup(this);

            group.MapGet(SearchTestTemplate, "");
            group.MapGet(GetTestTemplatePermissions, "/{testTemplateId}/Permissions");
            group.MapGet(GetTestTemplateDetail, "/{testTemplateId}");
            group.MapGet(GetSharingInTestTemplate, "/{testTemplateId}/Sharing");
            group.MapPost(AddSharingInTestTemplate, "/{testTemplateId}/Sharing");
            group.MapPost(CreateTestTemplate, "");
            group.MapDelete(DeleteTestTemplate, "/{testTemplateId}");
            group.MapPatch(UpdateTestTemplate, "/{testTemplateId}");

            var groupWithoutAntiforgery = app.MapGroup(this).DisableAntiforgery();
            groupWithoutAntiforgery.MapPost(ImportFileTestTemplate, "/ImportFile");
            groupWithoutAntiforgery.MapPatch(UpdateSharingInTestTemplate, "/{testTemplateId}/Sharing");
        }

        public async Task<Ok<ApiResponse<PaginatedList<TestTemplateDto>>>> SearchTestTemplate(
            [FromQuery] string? name,
            [FromQuery] string? shareMode,
            ISender sender,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var rq = new SearchTestTemplateQuery
            {
                TestTemplateName = name, PageNumber = pageNumber, PageSize = pageSize,ShareMode = shareMode
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
