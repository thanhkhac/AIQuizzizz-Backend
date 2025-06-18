using CleanArchitectureBase.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureBase.Infrastructure.Identity;

public class UserAccount : IdentityUser
{
    public bool IsDeleted { get; set; }
    public bool IsBanned { get; set; }

    public required User User { get; set; }
}
