using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Infrastructure;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    public CustomExceptionHandler()
    {
        // Register known exception types and handlers.
        _exceptionHandlers = new()
        {
            #region Code cũ
            // { typeof(ValidationException), HandleValidationException },
            // { typeof(NotFoundException), HandleNotFoundException },
            // { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
            // { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
            #endregion
            { typeof(ErrorCodeException), HandleErrorCodeException },
        };
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (_exceptionHandlers.ContainsKey(exceptionType))
        {
            await _exceptionHandlers[exceptionType].Invoke(httpContext, exception);
            return true;
        }

        return false;
    }

    private async Task HandleErrorCodeException(HttpContext httpContext, Exception ex)
    {
        var exception = (ErrorCodeException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object>()
            {
                Success = false,
                Data = null,
                Errors = exception.Errors,
                ValidationErrors = exception.ValidationErrors
            }
        );
    }

    #region Code cũ
    // private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    // {
    //     var exception = (ValidationException)ex;
    //
    //     httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    //
    //     await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(exception.Errors)
    //     {
    //         Status = StatusCodes.Status400BadRequest, Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    //     });
    // }
    //
    // private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    // {
    //     var exception = (NotFoundException)ex;
    //
    //     httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
    //
    //     await httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
    //     {
    //         Status = StatusCodes.Status404NotFound,
    //         Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    //         Title = "The specified resource was not found.",
    //         Detail = exception.Message
    //     });
    // }
    //
    // private async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    // {
    //     httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
    //
    //     await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object>()
    //         {
    //             Success = false,
    //             Data = null,
    //             Errors = new Dictionary<string, string[]>{ {"UNAUTHORIZED", new []{"Người dùng chưa xác thực"}}},
    //             ValidationErrors = new Dictionary<string, string[]>()
    //         }
    //     );
    // }
    //
    // private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    // {
    //     httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
    //
    //     await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
    //     {
    //         Status = StatusCodes.Status403Forbidden, Title = "Forbidden", Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
    //     });
    // }
    #endregion
}
