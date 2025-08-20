using System.Reflection;

namespace CleanArchitectureBase.Web.Infrastructure;

public static class MethodInfoExtensions
{
    //Kiểm tra xem hàm có phải Anoymous không (Anoymous method: hàm không tên, VD: app.MapGet("/test", () => "Hello");)
    public static bool IsAnonymous(this MethodInfo method)
    {
        var invalidChars = new[] { '<', '>' };
        return method.Name.Any(invalidChars.Contains);
    }

    public static void AnonymousMethod(this IGuardClause guardClause, Delegate input)
    {
        if (input.Method.IsAnonymous())
            throw new ArgumentException("The endpoint name must be specified when using anonymous handlers.");
    }
}
