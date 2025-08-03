using CleanArchitectureBase.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CleanArchitectureBase.Web.Attributes;

public class PaymentAuthEndpointFilter : IEndpointFilter
{
    private readonly IConfiguration _configuration;

    public PaymentAuthEndpointFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var expectedApiKey = _configuration["PaymentSettings:Apikey"];
       
       if(string.IsNullOrWhiteSpace(expectedApiKey))
           throw new ErrorCodeException("INTERNAL_SERVER_ERROR"); 

        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var apiKey) ||
            !apiKey.ToString().Equals($"Apikey {expectedApiKey}", StringComparison.OrdinalIgnoreCase))
        {
            throw new ErrorCodeException("COMMON_UNAUTHORIZED");
        }

        return await next(context);
    }
}
