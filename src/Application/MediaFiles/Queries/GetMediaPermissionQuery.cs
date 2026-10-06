using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Application.MediaFiles.Dtos;
using CleanArchitectureBase.Application.Plans.Service;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Application.MediaFiles.Queries;

/// <summary>
/// Quyền upload media của user hiện tại theo gói đang dùng (để UI ẩn/hiện nút upload)
/// </summary>
[Authorize]
public class GetMediaPermissionQuery : IRequest<MediaPermissionDto>
{
}

public class GetMediaPermissionQueryHandler : IRequestHandler<GetMediaPermissionQuery, MediaPermissionDto>
{
    private readonly IUser _user;
    private readonly IPlanService _planService;
    private readonly MediaSettings _settings;

    public GetMediaPermissionQueryHandler(IUser user, IPlanService planService, IOptions<MediaSettings> settings)
    {
        _user = user;
        _planService = planService;
        _settings = settings.Value;
    }

    public async Task<MediaPermissionDto> Handle(GetMediaPermissionQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId!.Value;
        return new MediaPermissionDto
        {
            CanUploadImage = await _planService.CanUploadImage(userId),
            CanUploadVideo = await _planService.CanUploadVideo(userId),
            MaxImageSizeMb = _settings.MaxImageSizeMb,
            MaxVideoSizeMb = _settings.MaxVideoSizeMb,
            MaxVideoSeconds = _settings.MaxVideoSeconds
        };
    }
}
