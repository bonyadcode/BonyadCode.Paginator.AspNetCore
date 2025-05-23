using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace BonyadCode.Paginator;

public static class PaginatorService
{

    public static Expression<Func<T, object>> GetTExpressionFunction<T>(string field)
    {
        var param = Expression.Parameter(typeof(T), "entity");
        var property =
            typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? typeof(T).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? typeof(T).GetProperty("DateCreated",
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
        {
            throw new ArgumentException($"Property '{field}' not found in type {typeof(T).Name}");
        }

        var propertyAccess = Expression.Property(param, property);
        var convert = Expression.Convert(propertyAccess, typeof(object));

        return Expression.Lambda<Func<T, object>>(convert, param);
    }
}