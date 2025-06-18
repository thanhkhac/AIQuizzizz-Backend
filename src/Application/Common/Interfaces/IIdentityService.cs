using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string email, string password);

    Task<Result> DeleteUserAsync(string userId);
    
    Task<TokenDto> TryLoginAsync(string email, string password);
    
}
