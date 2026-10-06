namespace CleanArchitectureBase.Application.Common.Extensions;

public static class FluentValidationExtensions
{
    public const int PasswordMinLength = 8;

    /// <summary>
    /// Quy tắc mật khẩu thống nhất (đăng ký / đặt lại / đổi / đặt mật khẩu): tối thiểu 8 ký tự, có ít nhất 1 chữ cái và 1 chữ số.
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeValidPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(PasswordMinLength).WithMessage("Mật khẩu phải có ít nhất 8 ký tự")
            .Matches("[A-Za-z]").WithMessage("Mật khẩu phải chứa ít nhất một chữ cái")
            .Matches("[0-9]").WithMessage("Mật khẩu phải chứa ít nhất một chữ số");
    }

    public static IRuleBuilderOptions<T, string?> NullOrNotEmpty<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(value =>
                value == null || value.Trim().Length > 0
            )
            .WithMessage("{PropertyName} must not be blank if provided");
    }
}
