using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Payments.Commands;
using CleanArchitectureBase.Application.Payments.Queries;
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

        group.MapPost(GetQrCode, "/QrCode");
    }

    public async Task<Ok<ApiResponse>> BuyPoint(
        [FromBody] BuyPointCommand command,
        ISender sender,
        HttpContext httpContext)
    {
        await sender.Send(command);
        return ApiResponse.SuccessResult().ToOk();
    }


    public async Task<Ok<ApiResponse<string>>> GetQrCode([FromQuery] int amount, ISender sender)
    {
        GetQrCodeQuery request = new GetQrCodeQuery
        {
            Amount = amount
        };
        var result =  await sender.Send(request);
        return result.ToOk();
    }
}
