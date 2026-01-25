using System.Linq.Expressions;

namespace Grad.CsLib.Data;

/// <summary>
/// Extension methods to perform Order By using strings
/// </summary>
public static class QueryableExtensions
{
    extension<T>(IQueryable<T> source)
    {
        public IOrderedQueryable<T> OrderByPath(string columnPath)
        {
            if (string.IsNullOrWhiteSpace(columnPath))
                throw new InvalidParameterException("Column path cannot be null or empty.");

            var columns = columnPath.Split(',').Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToArray();
            if (columns.Length == 0)
                throw new InvalidParameterException("No valid columns specified in column path.");

            IOrderedQueryable<T>? orderedQuery = null;
            foreach (var (col, idx) in columns.Select((c, i) => (c, i)))
            {
                var isDescending = col.StartsWith('-');
                var actualColumnPath = isDescending ? col[1..] : col;

                var parameter = Expression.Parameter(typeof(T), "item");
                Expression member;
                try
                {
                    member = actualColumnPath
                        .Split('.')
                        .Aggregate((Expression)parameter, Expression.PropertyOrField);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidParameterException($"Invalid field in column path '{actualColumnPath}'.", ex);
                }

                var keySelector = Expression.Lambda(member, parameter);
                var method = isDescending ? (idx == 0 ? "OrderByDescending" : "ThenByDescending") : (idx == 0 ? "OrderBy" : "ThenBy");
                var methodCall = Expression.Call(
                    typeof(Queryable),
                    method,
                    [parameter.Type, member.Type],
                    (orderedQuery == null ? source.Expression : orderedQuery.Expression),
                    Expression.Quote(keySelector));

                orderedQuery = (IOrderedQueryable<T>)source.Provider.CreateQuery(methodCall);
            }
            return orderedQuery!;
        }

        public IQueryable<T> ApplyFilters(object? filter, params string[] except)
        {
            if (filter == null)
                return source;

            var filterProperties = filter.GetType().GetProperties();
            foreach (var property in filterProperties)
            {
                if (except.Contains(property.Name)) continue;
                var value = property.GetValue(filter);
                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    var parameter = Expression.Parameter(typeof(T), "x");
                    var member = Expression.PropertyOrField(parameter, property.Name);
                    var constant = Expression.Constant(stringValue);
                    var body = Expression.Equal(member, constant);
                    var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
                    source = source.Where(predicate);
                }
                else if (value is IEnumerable<string> stringList && stringList.Any())
                {
                    var parameter = Expression.Parameter(typeof(T), "x");
                    var member = Expression.PropertyOrField(parameter, property.Name);
                    var constant = Expression.Constant(stringList);
                    var body = Expression.Call(typeof(Enumerable), "Contains", new[] { typeof(string) }, constant, member);
                    var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
                    source = source.Where(predicate);
                }
                else if (value is bool boolValue)
                {
                    var parameter = Expression.Parameter(typeof(T), "x");
                    var member = Expression.PropertyOrField(parameter, property.Name);
                    Expression body = boolValue
                        ? Expression.NotEqual(member, Expression.Constant(null))
                        : Expression.Equal(member, Expression.Constant(null));
                    var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
                    source = source.Where(predicate);
                }
            }
            return source;
        }

        public IQueryable<T> ApplySorting(PageOptions? pageOptions,
            Expression<Func<T, object>>? defaultSort = null,
            params Expression<Func<T, object>>[] thenBySort)
        {

            if (!string.IsNullOrEmpty(pageOptions?.SortBy))
            {
                return source.OrderByPath(pageOptions.SortBy);
            }

            if (defaultSort != null)
            {
                var ordered = source.OrderBy(defaultSort);
                return thenBySort.Aggregate(ordered, (current, expression) => current.ThenBy(expression));
            }
            return source;
        }

        public IQueryable<T> ApplyPaging(PageOptions? pageOptions,
            Expression<Func<T, object>>? defaultSort = null,
            params Expression<Func<T, object>>[] thenBySort)
        {
            if (pageOptions == null)
                return defaultSort != null ? source.OrderBy(defaultSort) : source;

            var result = source;

            result = result.Skip(pageOptions.PageOffset)
                .Take(pageOptions.PageSize);

            return result;
        }
    }
}
