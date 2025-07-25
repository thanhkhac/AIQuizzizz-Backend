namespace CleanArchitectureBase.Application.Users.Common;

public class GoogleUserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Picture { get; set; }
    public bool EmailVerified { get; set; }
} 