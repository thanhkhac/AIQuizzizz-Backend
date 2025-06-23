using CleanArchitectureBase.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Infrastructure.Identity;

public class UserAccount : IdentityUser<Guid>
{
    public bool IsDeleted { get; set; }
    public bool IsBanned { get; set; }

    public required User User { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() => this.Id = Guid.NewGuid();

    public ApplicationRole(string roleName)
        : this()
    {
        this.Name = roleName;
    }
}

public class ApplicationUserClaim : IdentityUserClaim<Guid>;

public class ApplicationUserLogin : IdentityUserLogin<Guid>;

public class ApplicationUserToken : IdentityUserToken<Guid>;

public class ApplicationUserRole : IdentityUserRole<Guid>;

public class ApplicationRoleClaim : IdentityRoleClaim<Guid>;
