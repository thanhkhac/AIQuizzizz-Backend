using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Web.Infrastructure;

public abstract class EndpointGroupBase
{
    public abstract void Map(WebApplication app);

}
