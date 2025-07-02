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
    private readonly IEmailService _emailService;

    public IdentityService(
        UserManager<UserAccount> userManager,
        IUserClaimsPrincipalFactory<UserAccount> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        SignInManager<UserAccount> signInManager,
        IConfiguration configuration,
        IOptions<JwtSettings> jwtSettings,
        ApplicationDbContext dbContext,
        IGoogleAuthService googleAuthService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
        _googleAuthService = googleAuthService;
        _emailService = emailService;
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
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_EMAIL_BANNED);
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
        GoogleUserDto? googleUser = null;
        try
        {
            googleUser =  await _googleAuthService.ExchangeCodeForUserInfoAsync(authorizationCode, redirectUri);
            if(googleUser == null) throw new Exception();
        }
        catch (Exception)
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS);
        }

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
            throw new Exception();
        }
        
        return await GenerateJwtTokenAsync(userAccount);
    }

    public async Task<TokenDto> TryGoogleRegisterAsync(string authorizationCode, string redirectUri)
    {
        GoogleUserDto? googleUser = null;
        try
        {
            googleUser =  await _googleAuthService.ExchangeCodeForUserInfoAsync(authorizationCode, redirectUri);
            if(googleUser == null) throw new Exception();
        }
        catch (Exception)
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS);
        }
        
        var existingUser = await _userManager.FindByEmailAsync(googleUser.Email);
        
        if (existingUser != null)
        {
            if (existingUser.IsDeleted)
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, $"User with email {googleUser.Email} not found");
                
            if (existingUser.IsBanned)
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_EMAIL_BANNED);
                
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
            EmailConfirmed = true 
        };

        var result = await _userManager.CreateAsync(userAccount);
        
        if (!result.Succeeded)
        {
            throw new ErrorCodeException(ErrorCodes.IDENTITY_DUPLICATE_EMAIL, "Failed to create user account");
        }
        
        var tokenDto = await GenerateJwtTokenAsync(userAccount);
        
        return tokenDto;
    }

    public async Task RequestEmailVerificationAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with email {email} not found");

        // Lockout gửi email
        const int MAX_EMAIL_REQUEST_ATTEMPTS = 5;
        const int EMAIL_REQUEST_LOCKOUT_MINUTES = 10;
        if (user.EmailRequestLockout >= MAX_EMAIL_REQUEST_ATTEMPTS &&
            user.EmailRequestLockoutTime.HasValue &&
            DateTime.UtcNow < user.EmailRequestLockoutTime.Value.AddMinutes(EMAIL_REQUEST_LOCKOUT_MINUTES))
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_TOO_MANY_REQUESTS, $"Quá nhiều yêu cầu gửi email. Vui lòng thử lại sau {EMAIL_REQUEST_LOCKOUT_MINUTES} phút.");
        }
        if (user.EmailRequestLockoutTime.HasValue &&
            DateTime.UtcNow >= user.EmailRequestLockoutTime.Value.AddMinutes(EMAIL_REQUEST_LOCKOUT_MINUTES))
        {
            user.EmailRequestLockout = 0;
        }

        // Lockout xác thực
        const int MAX_VERIFICATION_ATTEMPTS = 5;
        const int LOCKOUT_DURATION_MINUTES = 10;
        if (user.EmailVerificationLockout >= MAX_VERIFICATION_ATTEMPTS &&
            user.EmailVerificationCodeTime.HasValue &&
            DateTime.UtcNow < user.EmailVerificationCodeTime.Value.AddMinutes(LOCKOUT_DURATION_MINUTES))
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_TOO_MANY_REQUESTS, $"Tài khoản bị khóa do gửi quá nhiều mã xác thực. Vui lòng thử lại sau {LOCKOUT_DURATION_MINUTES} phút.");
        }
        if (user.EmailVerificationCodeTime.HasValue &&
            DateTime.UtcNow >= user.EmailVerificationCodeTime.Value.AddMinutes(LOCKOUT_DURATION_MINUTES))
        {
            user.EmailVerificationLockout = 0;
        }

        // Tạo và lưu mã xác thực
        user.EmailVerificationCode = GenerateRandomCode();
        user.EmailVerificationCodeTime = DateTime.UtcNow;
        user.EmailRequestLockout++;
        user.EmailRequestLockoutTime = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Gửi email xác thực
        const int VERIFICATION_CODE_EXPIRY_MINUTES = 10;
        var subject = "Xác thực email";
        var body = $@"<html><body><h2>Xác thực email</h2><p>Mã xác thực của bạn: <strong>{user.EmailVerificationCode}</strong></p><p>Mã xác thực sẽ hết hạn sau {VERIFICATION_CODE_EXPIRY_MINUTES} phút.</p><p>Nếu bạn không gửi yêu cầu xác thực, vui lòng bỏ qua email này.</p><br/><p>Trân trọng,<br/>AIQuizzizz</p></body></html>";
        await _emailService.SendEmailAsync(email, subject, body);
    }

    public async Task VerifyEmailAsync(EmailVerificationConfirmDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with email {dto.Email} not found");

        const int MAX_VERIFICATION_ATTEMPTS = 5;
        const int LOCKOUT_DURATION_MINUTES = 10;
        const int VERIFICATION_CODE_EXPIRY_MINUTES = 10;
        // Lockout
        if (user.EmailVerificationLockout >= MAX_VERIFICATION_ATTEMPTS &&
            user.EmailVerificationCodeTime.HasValue &&
            DateTime.UtcNow < user.EmailVerificationCodeTime.Value.AddMinutes(LOCKOUT_DURATION_MINUTES))
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_TOO_MANY_REQUESTS, $"Tài khoản bị khóa do nhập sai mã quá nhiều lần. Vui lòng thử lại sau {LOCKOUT_DURATION_MINUTES} phút.");
        }
        // Kiểm tra mã và thời gian hết hạn
        if (user.EmailVerificationCode != dto.VerificationCode ||
            !user.EmailVerificationCodeTime.HasValue ||
            DateTime.UtcNow > user.EmailVerificationCodeTime.Value.AddMinutes(VERIFICATION_CODE_EXPIRY_MINUTES))
        {
            user.EmailVerificationLockout++;
            await _userManager.UpdateAsync(user);
            if (user.EmailVerificationLockout >= MAX_VERIFICATION_ATTEMPTS)
            {
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_TOO_MANY_REQUESTS, $"Tài khoản bị khóa do nhập sai mã quá nhiều lần. Vui lòng thử lại sau {LOCKOUT_DURATION_MINUTES} phút.");
            }
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_VERIFICATION_CODE, "Mã xác thực không hợp lệ hoặc đã hết hạn.");
        }
        // Xác minh thành công
        user.EmailVerificationCode = null;
        user.EmailVerificationCodeTime = null;
        user.EmailVerificationLockout = 0;
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with email {dto.Email} not found");

        // Lockout gửi email
        const int MAX_EMAIL_REQUEST_ATTEMPTS = 3;
        const int EMAIL_REQUEST_LOCKOUT_MINUTES = 60;
        if (user.EmailRequestLockout >= MAX_EMAIL_REQUEST_ATTEMPTS &&
            user.EmailRequestLockoutTime.HasValue &&
            DateTime.UtcNow < user.EmailRequestLockoutTime.Value.AddMinutes(EMAIL_REQUEST_LOCKOUT_MINUTES))
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_TOO_MANY_REQUESTS, $"Quá nhiều yêu cầu gửi email. Vui lòng thử lại sau {EMAIL_REQUEST_LOCKOUT_MINUTES} phút.");
        }
        if (user.EmailRequestLockoutTime.HasValue &&
            DateTime.UtcNow >= user.EmailRequestLockoutTime.Value.AddMinutes(EMAIL_REQUEST_LOCKOUT_MINUTES))
        {
            user.EmailRequestLockout = 0;
        }

        // Lockout reset
        const int MAX_RESET_ATTEMPTS = 5;
        const int LOCKOUT_DURATION_MINUTES = 10;
        if (user.PasswordResetLockout >= MAX_RESET_ATTEMPTS &&
            user.PasswordResetCodeExpiryTime.HasValue &&
            DateTime.UtcNow < user.PasswordResetCodeExpiryTime.Value.AddMinutes(LOCKOUT_DURATION_MINUTES))
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_TOO_MANY_REQUESTS, $"Tài khoản bị khóa do gửi quá nhiều mã reset. Vui lòng thử lại sau {LOCKOUT_DURATION_MINUTES} phút.");
        }
        if (user.PasswordResetCodeExpiryTime.HasValue &&
            DateTime.UtcNow >= user.PasswordResetCodeExpiryTime.Value.AddMinutes(LOCKOUT_DURATION_MINUTES))
        {
            user.PasswordResetLockout = 0;
        }

        // Tạo mã reset ngẫu nhiên
        const int PASSWORD_RESET_CODE_EXPIRY_MINUTES = 10;
        var resetCode = GenerateRandomCode();
        user.PasswordResetCode = resetCode;
        user.PasswordResetCodeExpiryTime = DateTime.UtcNow;
        user.EmailRequestLockout++;
        user.EmailRequestLockoutTime = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Gửi email reset mật khẩu
        var subject = "Đặt lại mật khẩu";
        var body = $@"<html><body><h2>Đặt lại mật khẩu</h2><p>Mã đặt lại mật khẩu của bạn: <strong>{resetCode}</strong></p><p>Mã này sẽ hết hạn sau {PASSWORD_RESET_CODE_EXPIRY_MINUTES} phút.</p><p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p><br/><p>Trân trọng,<br/>AIQuizzizz</p></body></html>";
        await _emailService.SendEmailAsync(dto.Email, subject, body);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with email {dto.Email} not found");

        const int MAX_RESET_ATTEMPTS = 5;
        const int LOCKOUT_DURATION_MINUTES = 10;
        const int PASSWORD_RESET_CODE_EXPIRY_MINUTES = 10;
        // Lockout
        if (user.PasswordResetLockout >= MAX_RESET_ATTEMPTS &&
            user.PasswordResetCodeExpiryTime.HasValue &&
            DateTime.UtcNow < user.PasswordResetCodeExpiryTime.Value.AddMinutes(LOCKOUT_DURATION_MINUTES))
        {
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_TOO_MANY_REQUESTS, $"Tài khoản bị khóa do nhập sai mã quá nhiều lần. Vui lòng thử lại sau {LOCKOUT_DURATION_MINUTES} phút.");
        }
        // Kiểm tra mã reset và thời gian hết hạn
        if (user.PasswordResetCode != dto.ResetCode ||
            !user.PasswordResetCodeExpiryTime.HasValue ||
            DateTime.UtcNow > user.PasswordResetCodeExpiryTime.Value.AddMinutes(PASSWORD_RESET_CODE_EXPIRY_MINUTES))
        {
            user.PasswordResetLockout++;
            await _userManager.UpdateAsync(user);
            if (user.PasswordResetLockout >= MAX_RESET_ATTEMPTS)
            {
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_TOO_MANY_REQUESTS, $"Tài khoản bị khóa do nhập sai mã quá nhiều lần. Vui lòng thử lại sau {LOCKOUT_DURATION_MINUTES} phút.");
            }
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_INVALID_RESET_CODE, "Mã đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
        }
        // Đặt lại mật khẩu
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded) throw new ErrorCodeException(ErrorCodes.COMMON_BAD_REQUEST, $"Đổi mật khẩu thất bại: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiryTime = null;
        user.PasswordResetLockout = 0;
        await _userManager.UpdateAsync(user);
    }

    private string GenerateRandomCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
