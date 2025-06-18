namespace CleanArchitectureBase.Infrastructure.Identity;

public class RefreshToken
{
    public required string Id { get; set; }
    public required string UserAccountId { get; set; }
    public required string Token { get; set; }
    public required DateTime ExpireAt { get; set; }
    
    public UserAccount? UserAccount { get; set; }    
}
