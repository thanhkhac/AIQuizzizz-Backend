using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitectureBase.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<UserAccount> _userManager;
    private readonly IUserClaimsPrincipalFactory<UserAccount> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly SignInManager<UserAccount> _signInManager;
    private readonly JwtSettings _jwtSettings;


    public IdentityService(
        UserManager<UserAccount> userManager,
        IUserClaimsPrincipalFactory<UserAccount> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        SignInManager<UserAccount> signInManager,
        IConfiguration configuration,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<string?> GetUserNameAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user?.UserName;
    }

    public async Task<(Result Result, Guid UserId)> CreateUserAsync(string email, string password)
    {
        var user = new User { Id = Guid.NewGuid(), Email = email, FullName = email, };
        var userAccount = new UserAccount { Id = user.Id, UserName = Guid.NewGuid().ToString(), Email = email, User = user };

        var result = await _userManager.CreateAsync(userAccount, password);

        return (result.ToApplicationResult(), userAccount.Id);
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(Guid userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(UserAccount userAccount)
    {
        userAccount.IsDeleted = true;
        var result = await _userManager.UpdateAsync(userAccount);

        return result.ToApplicationResult();
    }

    public async Task<bool> IsLockedOutAsync(UserAccount userAccount)
    {
        return await _userManager.IsLockedOutAsync(userAccount);
    }


    public async Task<TokenDto> TryLoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || user.IsDeleted)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, $"User with email {email} not found");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (!result.Succeeded)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, "Incorrect password");

        return GenerateJwtToken(user);
    }

    private TokenDto GenerateJwtToken(UserAccount userAccount)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userAccount.Id.ToString()), };

        Guard.Against.NullOrEmpty(_jwtSettings.SecretKey, "Secret key is null or empty");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new TokenDto { AccessToken = new JwtSecurityTokenHandler().WriteToken(token), RefreshToken = "", ExpireMin = _jwtSettings.ExpiryMinutes };
    }


}
