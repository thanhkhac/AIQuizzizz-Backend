using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.zPlayGround;

[Authorize(Roles = Domain.Constants.Roles.Administrator)]
public class ZPlayGroundQuery : IRequest<string>
{
    
}


public class ZPlayGroundQueryHandler : IRequestHandler<ZPlayGroundQuery, string>
{
    private readonly IGoogleAccessTokenProvider  _googleAccessTokenProvider;
    public ZPlayGroundQueryHandler(IGoogleAccessTokenProvider googleAccessTokenProvider)
    {
        _googleAccessTokenProvider = googleAccessTokenProvider;
    }
    public Task<string> Handle(ZPlayGroundQuery request, CancellationToken cancellationToken)
    {
        return _googleAccessTokenProvider.GetAccessTokenAsync();
    }
}
