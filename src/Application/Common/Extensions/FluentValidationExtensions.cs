namespace CleanArchitectureBase.Application.Common.Extensions;

public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> NullOrNotEmpty<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Must(value =>
                value == null || value.Trim().Length > 0
            )
            .WithMessage("{PropertyName} must not be blank if provided");
    }
}
