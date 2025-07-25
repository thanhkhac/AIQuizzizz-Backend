namespace CleanArchitectureBase.Infrastructure.Identity;

public class RefreshToken
{
    public required string Id { get; set; }
    public required Guid UserAccountId { get; set; }
    public required string Token { get; set; }
    public required DateTimeOffset ExpireAt { get; set; }
    
    public UserAccount? UserAccount { get; set; }    
}
