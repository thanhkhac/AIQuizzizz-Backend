using System.Security.Claims;

using CleanArchitectureBase.Application.Common.Interfaces;

namespace CleanArchitectureBase.Web.Services;

//Vai trò: Dùng để truy xuất thông tin của người dùng hiện tại (Id)
public class CurrentUser : IUser
{
    //IHttpContextAccessor: Service cho phép truy cập vào HttpContext của request hiện tại
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
