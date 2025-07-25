namespace CleanArchitectureBase.Application.Users.Common;

public class EmailVerificationConfirmDto
{
    public required string Email { get; set; }
    public required string VerificationCode { get; set; }
} 