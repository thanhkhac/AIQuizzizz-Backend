using System.Diagnostics;
using CleanArchitectureBase.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureBase.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public PerformanceBehaviour(
        ILogger<TRequest> logger,
        IUser user,
        IIdentityService identityService)
    {
        _timer = new Stopwatch();

        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine("PerformanceBehaviour");

        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;
        Console.WriteLine($"Request:  ({elapsedMilliseconds} milliseconds)");
        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _user.UserId ?? Guid.Empty;
            var userName = string.Empty;

            if (userId != Guid.Empty)
            {
                userName = await _identityService.GetUserNameAsync(userId);
            }

            _logger.LogWarning("CleanArchitectureBase Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                requestName, elapsedMilliseconds, userId, userName, request);
        }

        return response;
    }
}
