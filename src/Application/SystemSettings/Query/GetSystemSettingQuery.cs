using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.SystemSettings.Dto;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.SystemSettings.Query;

public class GetSystemSettingQuery : IRequest<SystemSettingDetailDto> { }

public class GetSystemSettingQueryHandler : IRequestHandler<GetSystemSettingQuery, SystemSettingDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    
    public GetSystemSettingQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }
    
    public async Task<SystemSettingDetailDto> Handle(GetSystemSettingQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator,
            Domain.Constants.Roles.Moderator);
        if (!isAdmin)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION);

        var systemSetting = await _context.SystemSettings
            .OrderByDescending(x => x.Created)
            .FirstOrDefaultAsync(cancellationToken);
        if (systemSetting == null)
            throw new ErrorCodeException(ErrorCodes.SYSTEM_SETTING_NOT_FOUND);

        return new SystemSettingDetailDto
        {
            Id = systemSetting.Id,
            InputCostPerMillionTokens = systemSetting.InputCostPerMillionTokens,
            OutputCostPerMillionTokens = systemSetting.OutputCostPerMillionTokens,
            FixedSystemFee = systemSetting.FixedSystemFee,
            MaxInputToken = systemSetting.MaxInputToken,
            MaxOutputToken = systemSetting.MaxOutputToken
        };
    }
}

