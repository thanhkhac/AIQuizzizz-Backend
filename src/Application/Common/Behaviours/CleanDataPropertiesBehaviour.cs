using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using CleanArchitectureBase.Application.Common.Atributes;
using MediatR;

namespace CleanArchitectureBase.Application.Common.Behaviours;

public class CleanDataPropertiesBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int MaxDepth = 5;
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
    private static readonly ConcurrentDictionary<Type, Type> _elementTypeCache = new();

    public async Task<TResponse> Handle(
        TRequest? request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("CleanDataPropertiesBehaviour");
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

        // Lấy properties từ cache
        var properties = _propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        foreach (var prop in properties)
        {
            if (prop.IsDefined(typeof(NoTrimRecursiveAttribute), inherit: true))
                continue;

            if (!prop.CanRead || !prop.CanWrite)
                continue;

            if (prop.GetIndexParameters().Length > 0)
                continue;

            if (prop.PropertyType == typeof(string))
            {
                var value = (string?)prop.GetValue(obj);
                if (!string.IsNullOrWhiteSpace(value))
                    prop.SetValue(obj, value.Trim());
            }
            else if (
                typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType)
                && prop.PropertyType != typeof(string))
            {
                var collection = (System.Collections.IEnumerable?)prop.GetValue(obj);
                if (collection != null)
                {
                    var elementType = GetCollectionElementType(prop.PropertyType);
                    if (elementType == typeof(string) && prop.PropertyType == typeof(List<string>))
                    {
                        var trimmedList = new List<string>();
                        foreach (var item in collection)
                        {
                            if (item is string str && !string.IsNullOrWhiteSpace(str))
                                trimmedList.Add(str.Trim());
                        }
                        prop.SetValue(obj, trimmedList);
                    }
                    else
                    {
                        foreach (var item in collection)
                        {
                            TrimStrings(item, depth + 1);
                        }
                    }
                }
            }
            else if (!prop.PropertyType.IsValueType && !prop.PropertyType.IsEnum)
            {
                var nestedValue = prop.GetValue(obj);
                if (nestedValue != null)
                    TrimStrings(nestedValue, depth + 1);
            }
        }
    }

    private Type? GetCollectionElementType(Type type)
    {
        // Kiểm tra nếu type triển khai IEnumerable<T>
        var enumerableType = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerableType != null) { return enumerableType.GetGenericArguments()[0]; }

        return null;
    }
}
