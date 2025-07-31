using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;
using FluentValidation.Results;

namespace CleanArchitectureBase.Application.Common.Exceptions;

public class ErrorCodeException : Exception
{
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    public IDictionary<string, string[]> ValidationErrors { get; set; } = new Dictionary<string, string[]>();


    public ErrorCodeException(string errorCode)
    {
        Errors[errorCode] = new[] { $"Error occurred with code: {errorCode}" };
    }

    public ErrorCodeException(string errorCode, string message)
        : base(message)
    {
        Errors[errorCode] = new[] { message };
    }
    
    public ErrorCodeException(string[] errors)
    {
        Errors = errors.ToDictionary(
            error => error,
            error => new string[0]
        );
    }

    public ErrorCodeException(IDictionary<string, string[]> errors)
    {
        Errors = errors;
    }
    
    public ErrorCodeException(IEnumerable<ValidationFailure> validationFailures)
    {
        Errors[ErrorCodes.COMMON_INVALID_MODEL] = new[] {"Invalid model"};
        ValidationErrors = validationFailures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}
