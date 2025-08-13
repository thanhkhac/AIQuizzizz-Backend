using System.Reflection;
using CleanArchitectureBase.Application.Common.Atributes;

namespace CleanArchitectureBase.Application.Common.Behaviours;

public class TrimStringPropertiesBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int MaxDepth = 5;

    public async Task<TResponse> Handle(
        TRequest? request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("TrimStringPropertiesBehaviour");
        if (request != null)
        {
            TrimStrings(request, 0);
        }

        return await next();
    }

    private void TrimStrings(object? obj, int depth)
    {
        // Điều kiện dừng
        if (obj == null || depth > MaxDepth)
            return;

        // Lấy kiểu của request
        var type = obj.GetType();

        // Bỏ qua các kiểu nguyên thủy
        if (type.IsPrimitive || type.IsEnum || type == typeof(decimal) || type == typeof(DateTime))
            return;

        if (type == typeof(string))
            return;

        // Public: lấy các property có access modifier laf public
        // Instance: lấy thành phần thuộc về một instance (không phải static), một instance là một đối tượng cụ thể được tạo bằng new
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.IsDefined(typeof(NoTrimRecursiveAttribute),
                    inherit: true)) // inherit: Kiểm tra xem property ở lớp cha có gắn attribute không (nếu override)
                continue;

            if (!prop.CanRead || !prop.CanWrite)
                continue;

            if (prop.PropertyType == typeof(string))
            {
                var value = (string?)prop.GetValue(obj);
                if (value != null)
                    prop.SetValue(obj, value.Trim());
            }
            else if (!prop.PropertyType.IsValueType && !prop.PropertyType.IsEnum)
            {
                var nestedValue = prop.GetValue(obj);
                if (nestedValue != null)
                    TrimStrings(nestedValue, depth + 1);
            }
            else if (
                typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType)
                && prop.PropertyType != typeof(string))
            {
                var collection = (System.Collections.IEnumerable?)prop.GetValue(obj);
                if (collection != null)
                {
                    foreach (var item in collection)
                    {
                        TrimStrings(item, depth + 1);
                    }
                }
            }
        }
    }
}
