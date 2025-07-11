namespace CleanArchitectureBase.Application.Users.Common;

public class CreatedByDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}
