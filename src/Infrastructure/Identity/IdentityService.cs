using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Infrastructure.Data;
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
    private readonly ApplicationDbContext _dbContext;
    private readonly IGoogleAuthService _googleAuthService;

    public IdentityService(
        UserManager<UserAccount> userManager,
        IUserClaimsPrincipalFactory<UserAccount> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        SignInManager<UserAccount> signInManager,
        IConfiguration configuration,
        IOptions<JwtSettings> jwtSettings,
        ApplicationDbContext dbContext,
        IGoogleAuthService googleAuthService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
        _googleAuthService = googleAuthService;
    }

    public async Task<string?> GetUserNameAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user?.UserName;
    }

    public async Task<(Result Result, Guid UserId)> CreateUserAsync(string email, string password)
    {
    
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            if (existingUser.IsBanned)
            {
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_BANNED);
            }
            throw new ErrorCodeException(ErrorCodes.IDENTITY_DUPLICATE_EMAIL);
        }
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = email,
        };
        var userAccount = new UserAccount
        {
            Id = user.Id,
            UserName = Guid.NewGuid().ToString(),
            Email = email,
            User = user
        };

        IdentityResult result = await _userManager.CreateAsync(userAccount, password);

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

        if (user.IsBanned)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_BANNED);
            
        if(user.EmailConfirmed == false) throw new ErrorCodeException(ErrorCodes.ACCOUNT_EMAIL_NOT_VERIFIED);    

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        
        if(result.IsLockedOut)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_LOCKED_OUT);
        if (!result.Succeeded)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, "Incorrect password");

        return await GenerateJwtTokenAsync(user);
    }

    public async Task<List<Guid>> GetUsersInRoleAsync()
    {
        var admin = await _userManager.GetUsersInRoleAsync(Roles.Administrator);
        return admin.Select(u => u.Id).ToList();
    }
    
    public async Task<Guid> ChangeRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with id {userId} not found");
        
        var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
    
        if (currentRole == role)
        {
            return userId;
        }
        
        if (currentRole != null)
        {
          await _userManager.RemoveFromRoleAsync(user, currentRole);
        }

        await _userManager.AddToRoleAsync(user, role);
        await _userManager.UpdateSecurityStampAsync(user);

        return userId;
    }
    
    private async Task<TokenDto> GenerateJwtTokenAsync(UserAccount userAccount)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userAccount.Id.ToString()),
        };

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
        var refreshToken = await GenerateRefreshTokenAsync(userAccount);
        return new TokenDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken.Token,
            ExpireMin = _jwtSettings.ExpiryMinutes
        };
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, Guid userId)
    {
        var storedRefreshToken = await _dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserAccountId == userId);

        if (storedRefreshToken != null)
        {
            _dbContext.Set<RefreshToken>().Remove(storedRefreshToken);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "Refresh token not found");
        }
    }

    public async Task<TokenDto> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var storedRefreshToken = await _dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedRefreshToken == null || storedRefreshToken.ExpireAt < DateTime.UtcNow)
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, "Invalid or expired refresh token");
        }

        var principal = GetPrincipalFromToken(accessToken, validateLifetime: false);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId) || userId != storedRefreshToken.UserAccountId)
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, "Invalid access token for refresh");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted || user.IsBanned || await _userManager.IsLockedOutAsync(user))
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, "User account is invalid or locked out or banned");
        }

        _dbContext.Set<RefreshToken>().Remove(storedRefreshToken);
        await _dbContext.SaveChangesAsync();

        return await GenerateJwtTokenAsync(user);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(UserAccount userAccount)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            UserAccountId = userAccount.Id,
            Token = GenerateSecureToken(),
            ExpireAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        };

        _dbContext.Set<RefreshToken>().Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return refreshToken;
    }


    private string GenerateSecureToken()
    {
        return Guid.NewGuid().ToString();
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        Guard.Against.NullOrEmpty(_jwtSettings.SecretKey, "Secret key is null or empty");
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = validateLifetime,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out var validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<TokenDto> TryGoogleLoginAsync(string authorizationCode, string redirectUri)
    {
        var googleUser = await _googleAuthService.ExchangeCodeForUserInfoAsync(authorizationCode, redirectUri);
        
        var existingUser = await _userManager.FindByEmailAsync(googleUser.Email);
        
        if (existingUser != null)
        {
            if (existingUser.IsDeleted)
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, $"User with email {googleUser.Email} not found");
                
            if (existingUser.IsBanned)
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_BANNED);
                
            if (!existingUser.EmailConfirmed)
            {
                existingUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(existingUser);
            }
            
            return await GenerateJwtTokenAsync(existingUser);
        }
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = googleUser.Email,
            FullName = googleUser.Name,
        };
        
        var userAccount = new UserAccount
        {
            Id = user.Id,
            UserName = Guid.NewGuid().ToString(),
            Email = googleUser.Email,
            User = user,
            EmailConfirmed = true 
        };

        var result = await _userManager.CreateAsync(userAccount);
        
        if (!result.Succeeded)
        {
            throw new ErrorCodeException(ErrorCodes.IDENTITY_DUPLICATE_EMAIL, "Failed to create user account");
        }
        
        return await GenerateJwtTokenAsync(userAccount);
    }

    public async Task<TokenDto> TryGoogleRegisterAsync(string authorizationCode, string redirectUri)
    {
        var googleUser = await _googleAuthService.ExchangeCodeForUserInfoAsync(authorizationCode, redirectUri);
        
        var existingUser = await _userManager.FindByEmailAsync(googleUser.Email);
        
        if (existingUser != null)
        {
            if (existingUser.IsDeleted)
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, $"User with email {googleUser.Email} not found");
                
            if (existingUser.IsBanned)
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_BANNED);
                
            if (!existingUser.EmailConfirmed)
            {
                existingUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(existingUser);
            }
            
            var token = await GenerateJwtTokenAsync(existingUser);
            
            return token;
        }
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = googleUser.Email,
            FullName = googleUser.Name,
        };
        
        var userAccount = new UserAccount
        {
            Id = user.Id,
            UserName = Guid.NewGuid().ToString(),
            Email = googleUser.Email,
            User = user,
            EmailConfirmed = true // Google emails are pre-verified
        };

        var result = await _userManager.CreateAsync(userAccount);
        
        if (!result.Succeeded)
        {
            throw new ErrorCodeException(ErrorCodes.IDENTITY_DUPLICATE_EMAIL, "Failed to create user account");
        }
        
        var tokenDto = await GenerateJwtTokenAsync(userAccount);
        
        return tokenDto;
    }

}
