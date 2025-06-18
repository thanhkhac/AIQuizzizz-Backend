using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Infrastructure.Identity;

// public class CustomSignInManager : SignInManager<ApplicationUser>
// {
//     public CustomSignInManager(UserManager<ApplicationUser> userManager,
//         IHttpContextAccessor contextAccessor,
//         IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
//         IOptions<IdentityOptions> optionsAccessor,
//         ILogger<SignInManager<ApplicationUser>> logger,
//         IAuthenticationSchemeProvider schemes,
//         IUserConfirmation<ApplicationUser> confirmation)
//         : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
//     {
//     }
//
     // public override async Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure)
     // {
     //     if (user.IsBanned)
     //     {
     //         return SignInResult.NotAllowed; 
     //     }
     //
     //     return await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
     // }
// }
