using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsDeleted { get; set; }
    public bool IsBanned { get; set; }
}
