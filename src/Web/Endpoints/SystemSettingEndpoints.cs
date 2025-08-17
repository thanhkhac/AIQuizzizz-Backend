using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.SystemSettings.Commands;
using CleanArchitectureBase.Application.SystemSettings.Dto;
using CleanArchitectureBase.Application.SystemSettings.Query;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class SystemSettingEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);
        
        group.MapGet(GetSystemSetting, "");
        group.MapPost(CreateSystemSetting, "");
    }
    
    public async Task<Ok<ApiResponse<Guid>>> CreateSystemSetting([FromBody] CreateSystemSettingCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<SystemSettingDetailDto>>> GetSystemSetting(ISender sender)
    {
        var result = await sender.Send(new GetSystemSettingQuery());
        return result.ToOk();
    }
}
