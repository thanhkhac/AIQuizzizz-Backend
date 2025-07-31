using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Payments.Commands;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Application.Plans.Dto;
using CleanArchitectureBase.Web.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class PaymentEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost(BuyPoint, "/WebHook/Sepay")
            .AddEndpointFilter<PaymentAuthEndpointFilter>();

        group.MapGet(Test);
    }

    public async Task<Ok<ApiResponse>> BuyPoint(
        [FromBody] BuyPointCommand command,
        ISender sender,
        HttpContext httpContext)
    {
        await sender.Send(command);
        return ApiResponse.SuccessResult().ToOk();
    }
    
    
    public async Task<Ok<ApiResponse>> Test()
    {
        await Task.CompletedTask;
        return  ApiResponse.SuccessResult().ToOk();
    }
}
