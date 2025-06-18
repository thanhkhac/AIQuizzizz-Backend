namespace CleanArchitectureBase.Domain.Entities;

public class User 
{
    public required string Id { get; set; } = Guid.NewGuid().ToString();
    public string? FullName { get; set; } 
    public required string Email { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsBanned { get; set; }
    
}

