using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(Guid userId);

    Task<bool> IsInRoleAsync(Guid userId, string role);

    Task<bool> AuthorizeAsync(Guid userId, string policyName);

    Task<(Result Result, Guid UserId)> CreateUserAsync(string email, string password);

    Task<Result> DeleteUserAsync(Guid userId);
    
    Task<TokenDto> TryLoginAsync(string email, string password);

    Task<List<Guid>> GetUsersInRoleAsync();
}
