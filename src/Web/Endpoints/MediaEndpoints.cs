using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.MediaFiles.Commands;
using CleanArchitectureBase.Application.MediaFiles.Dtos;
using CleanArchitectureBase.Application.MediaFiles.Queries;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class MediaEndpoints : EndpointGroupBase
{
    // Giới hạn cứng của request (validate chi tiết theo loại file nằm trong command)
    private const long MaxRequestBytes = 210L * 1024 * 1024;

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapGet(GetMediaPermissions, "Permissions");

        group.MapPost(UploadMedia, "Upload")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MaxRequestBytes });
    }

    /// <summary>
    /// Quyền upload ảnh/video theo gói của user hiện tại
    /// </summary>
    public async Task<Ok<ApiResponse<MediaPermissionDto>>> GetMediaPermissions(ISender sender)
    {
        var result = await sender.Send(new GetMediaPermissionQuery());
        return result.ToOk();
    }

    /// <summary>
    /// Upload 1 ảnh hoặc video (multipart field "file"). Trả về mediaId để gắn vào câu hỏi + presigned URL ngắn hạn để preview
    /// </summary>
    public async Task<Ok<ApiResponse<UploadMediaResultDto>>> UploadMedia([FromForm] IFormFile file, ISender sender)
    {
        await using var stream = file.OpenReadStream();
        var result = await sender.Send(new UploadMediaCommand
        {
            File = new FileStreamData
            {
                Data = stream,
                ContentType = file.ContentType,
                FileName = file.FileName
            },
            Length = file.Length
        });
        return result.ToOk();
    }
}
