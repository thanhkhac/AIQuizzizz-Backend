using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Application.Plans.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class PlanEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapGet(GetDetailPlan, "/{planId}");
        group.MapPost(CreatePlan, "");
        group.MapPost(BuyPlan, "/{planId}/Buy");
        group.MapDelete(DeletePlan, "/{planId}");
    }
    
    public async Task<Ok<ApiResponse<PlanDetailDto>>> GetDetailPlan(
        [FromRoute] Guid planId,
        ISender sender)
    {
        var rq = new GetDetailPlanQuery { PlanId = planId};
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> BuyPlan(
        [FromRoute] Guid planId,
        ISender sender)
    {
        var rq = new BuyPlanCommand { PlanId = planId};
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> DeletePlan(
        [FromRoute] Guid planId,
        ISender sender)
    {
        var rq = new DeletePlanCommand { PlanId = planId};
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> CreatePlan(
        [FromBody] CreatePlanCommand rq,
        ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
