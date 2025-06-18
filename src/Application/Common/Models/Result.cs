namespace CleanArchitectureBase.Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IDictionary<string, string[]> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; init; }

    public IDictionary<string, string[]> Errors { get; init; }

    public static Result Success()
    {
        return new Result(true, new Dictionary<string, string[]>());
    }

    public static Result Failure(IDictionary<string, string[]> errors)
    {
        return new Result(false, errors);
    }

    public static Result Failure(string errorCode, string message)
    {
        return new Result(false, new Dictionary<string, string[]> { { errorCode, new[] { message } } });
    }

    public static Result Failure(string errorCode)
    {
        return new Result(false, new Dictionary<string, string[]> ());
    }
}
