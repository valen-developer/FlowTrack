using System.Linq.Expressions;
using System.Reflection;
using FlowTrack.Shared.Domain.FilterCriterias;

namespace FlowTrack.Shared.Infrastructure.Persistence
{
    public static class EfFilterCriteriaConverter
    {
        public static IQueryable<T> Apply<T>(IQueryable<T> query, FilterCriteria criteria)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(criteria);

            if (criteria.Filters is { Groups.Count: > 0 })
            {
                var predicate = BuildPredicate<T>(criteria.Filters);
                query = query.Where(predicate);
            }

            if (
                criteria.Order is { OrderBy.PropertyName.Length: > 0 }
                && criteria.Order.OrderType.Type != OrderTypes.NONE
            )
            {
                query = ApplyOrdering(query, criteria.Order);
            }

            if (criteria.Offset.HasValue)
                query = query.Skip(criteria.Offset.Value);

            if (criteria.Limit.HasValue)
                query = query.Take(criteria.Limit.Value);

            return query;
        }

        private static Expression<Func<T, bool>> BuildPredicate<T>(Filters filters)
        {
            var parameter = Expression.Parameter(typeof(T), "e");

            Expression? body = null;

            foreach (var group in filters.Groups)
            {
                Expression? groupBody = null;

                foreach (var filter in group)
                {
                    var filterExpr = BuildFilterExpression(parameter, filter);
                    groupBody = groupBody is null
                        ? filterExpr
                        : Expression.AndAlso(groupBody, filterExpr);
                }

                body = body is null ? groupBody : Expression.OrElse(body!, groupBody!);
            }

            return Expression.Lambda<Func<T, bool>>(body!, parameter);
        }

        private static Expression BuildFilterExpression(
            ParameterExpression parameter,
            Filter filter
        )
        {
            var property = AccessProperty(parameter, filter.Field.FieldName);
            var targetType = property.Type;

            var isNullable =
                targetType.IsGenericType
                && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
            var underlyingType = isNullable ? Nullable.GetUnderlyingType(targetType)! : targetType;
            var parsedValue = ParseValue(filter.Value.FieldValue, underlyingType);

            var constant = Expression.Constant(
                parsedValue,
                isNullable ? targetType : underlyingType
            );

            return filter.Operator.Type switch
            {
                FilterOperators.Equals => Expression.Equal(property, constant),
                FilterOperators.NotEquals => Expression.NotEqual(property, constant),
                FilterOperators.GreaterThan => Expression.GreaterThan(property, constant),
                FilterOperators.GreaterThanOrEqual => Expression.GreaterThanOrEqual(
                    property,
                    constant
                ),
                FilterOperators.LessThan => Expression.LessThan(property, constant),
                FilterOperators.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
                FilterOperators.Contains => BuildStringMethodCall(
                    property,
                    constant,
                    nameof(string.Contains)
                ),
                FilterOperators.StartsWith => BuildStringMethodCall(
                    property,
                    constant,
                    nameof(string.StartsWith)
                ),
                FilterOperators.EndsWith => BuildStringMethodCall(
                    property,
                    constant,
                    nameof(string.EndsWith)
                ),
                _ => throw new NotSupportedException(
                    $"Filter operator '{filter.Operator.Type}' is not supported."
                ),
            };
        }

        private static Expression AccessProperty(Expression expression, string propertyPath)
        {
            var current = expression;
            foreach (var part in propertyPath.Split('.'))
            {
                var prop =
                    current.Type.GetProperty(
                        part,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                    )
                    ?? throw new InvalidOperationException(
                        $"Property '{part}' not found on type '{current.Type.Name}'."
                    );
                current = Expression.Property(current, prop);
            }
            return current;
        }

        private static Expression BuildStringMethodCall(
            Expression property,
            Expression constant,
            string methodName
        )
        {
            var method =
                typeof(string).GetMethod(methodName, [typeof(string)])
                ?? throw new InvalidOperationException(
                    $"Method '{methodName}' not found on string."
                );
            return Expression.Call(property, method, constant);
        }

        private static object ParseValue(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;
            if (targetType == typeof(Guid))
                return Guid.Parse(value);
            if (targetType == typeof(bool))
                return bool.Parse(value);
            if (targetType == typeof(int))
                return int.Parse(value);
            if (targetType == typeof(long))
                return long.Parse(value);
            if (targetType == typeof(short))
                return short.Parse(value);
            if (targetType == typeof(byte))
                return byte.Parse(value);
            if (targetType == typeof(float))
                return float.Parse(value);
            if (targetType == typeof(double))
                return double.Parse(value);
            if (targetType == typeof(decimal))
                return decimal.Parse(value);
            if (targetType == typeof(DateTime))
                return DateTime.Parse(value);
            if (targetType == typeof(DateOnly))
                return DateOnly.Parse(value);
            if (targetType == typeof(TimeOnly))
                return TimeOnly.Parse(value);
            if (targetType == typeof(TimeSpan))
                return TimeSpan.Parse(value);
            if (targetType == typeof(char))
                return value[0];
            if (targetType.IsEnum)
                return Enum.Parse(targetType, value);
            return System.Convert.ChangeType(value, targetType);
        }

        private static IQueryable<T> ApplyOrdering<T>(IQueryable<T> query, Order order)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = AccessProperty(parameter, order.OrderBy.PropertyName);
            var lambda = Expression.Lambda(property, parameter);

            var methodName =
                order.OrderType.Type == OrderTypes.ASC ? "OrderBy" : "OrderByDescending";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                [typeof(T), property.Type],
                query.Expression,
                Expression.Quote(lambda)
            );

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
