namespace CleanArchitectureBase.Application.Users.Common;

public class TokenDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required int ExpireMin { get; set; }
    public bool HasPassword { get; set; } = true;
}
