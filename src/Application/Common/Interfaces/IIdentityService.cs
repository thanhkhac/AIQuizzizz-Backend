using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.Users.Common;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(Guid userId);

    Task<bool> IsInRoleAsync(Guid userId, string role);

    Task<bool> AuthorizeAsync(Guid userId, string policyName);

    Task<(Result Result, Guid UserId)> CreateUserAsync(string email, string password);

    Task<Result> DeleteUserAsync(Guid userId);
    
    Task<TokenDto> TryLoginAsync(string email, string password);
    
    Task<TokenDto> TryGoogleLoginAsync(string authorizationCode, string redirectUri);
    
    Task TrySetPasswordAsync(Guid userId, string password);
    
    Task<TokenDto> RefreshTokenAsync(string accessToken, string refreshToken);
    
    Task RevokeRefreshTokenAsync(string refreshToken, Guid userId);
    
    Task<List<Guid>> GetUsersInRoleAsync();
    
    Task<Guid> ChangeRoleAsync(Guid userId, string role,  List<string>? protectedRoles = null);

    Task RequestEmailVerificationAsync(string email);
    
    Task VerifyEmailAsync(EmailVerificationConfirmDto dto);
    
    Task RequestPasswordResetAsync(ForgotPasswordDto dto);
    
    Task ResetPasswordAsync(ResetPasswordDto dto);
    
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

    Task BanUser(Guid userId, bool isBan);
        
    Task<bool> IsInAnyRoleAsync(Guid userId, params string[] roles);
    
    Task<IList<string>> GetUserRolesAsync(Guid userId);

    Task<PaginatedList<UserForListDto>> SearchUserWithRole(SearchUserDto dto);
    
    
}
