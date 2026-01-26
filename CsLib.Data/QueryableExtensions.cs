using System.Linq.Expressions;

namespace Grad.CsLib.Data;

/// <summary>
/// Extension methods to perform Order By using strings
/// </summary>
public static class QueryableExtensions
{
    extension<T>(IQueryable<T> source)
    {
        /// <summary>
        /// Orders an <see cref="IQueryable{T}"/> source based on a specified column path string.
        /// The column path can include multiple column names separated by commas,
        /// with optional '-' prefix for descending order.
        /// </summary>
        /// <param name="columnPath">
        /// A string representing the column path(s) to sort by.
        /// Columns are separated by commas, and a '-' prefix indicates descending order for a column.
        /// </param>
        /// <returns>
        /// An <see cref="IOrderedQueryable{T}"/> that is ordered based on the column path string provided.
        /// </returns>
        /// <exception cref="InvalidParameterException">
        /// Thrown when the column path is null, empty, or contains invalid fields.
        /// </exception>
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

        /// <summary>
        /// Applies a set of filters to an <see cref="IQueryable{T}"/> source based on the specified filter object.
        /// Filters are derived from the properties of the filter object, where non-null, non-empty values
        /// indicate conditions for inclusion. String values are matched exactly, while enumerable string
        /// values are checked for containment. Boolean values determine the presence or absence of a field.
        /// </summary>
        /// <param name="filter">
        /// An object containing properties used to filter the source. Each property name is matched
        /// to a corresponding property in the source type to form filtering conditions.
        /// </param>
        /// <param name="except">
        /// A list of property names to exclude from the filtering process, even if they exist in the filter object.
        /// </param>
        /// <returns>
        /// A filtered <see cref="IQueryable{T}"/> sequence, with conditions applied based on the filter object and exclusions.
        /// </returns>
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

        /// <summary>
        /// Applies sorting to an <see cref="IQueryable{T}"/> source based on the provided <see cref="PageOptions"/>.
        /// If the <c>SortBy</c> property in the <paramref name="pageOptions"/> is specified,
        /// sorting is performed using the column(s) defined in it. If no <c>SortBy</c> is provided,
        /// the default and optional sorting expressions are applied.
        /// </summary>
        /// <param name="pageOptions">
        /// The <see cref="PageOptions"/> object containing sorting preferences, including the column(s)
        /// to sort by.
        /// </param>
        /// <param name="defaultSort">
        /// An optional default sorting expression to use when no <c>SortBy</c> is defined in the
        /// <paramref name="pageOptions"/>.
        /// </param>
        /// <param name="thenBySort">
        /// Optional additional sorting expressions to be applied in sequence after the default sorting.
        /// </param>
        /// <returns>
        /// An <see cref="IQueryable{T}"/> that is sorted based on the specified <paramref name="pageOptions"/>,
        /// or using the provided <paramref name="defaultSort"/> and <paramref name="thenBySort"/> parameters if
        /// <c>SortBy</c> is not specified.
        /// </returns>
        public IQueryable<T> ApplySorting(PageOptions? pageOptions,
            Expression<Func<T, object>>? defaultSort = null,
            params Expression<Func<T, object>>[] thenBySort
        )
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

        /// <summary>
        /// Applies paging to the provided <see cref="IQueryable{T}"/> source based on the given page options.
        /// If the page options are null, the source is returned as is. Otherwise, the results are paginated
        /// according to the specified offset and page size.
        /// </summary>
        /// <param name="pageOptions">
        /// An instance of <see cref="PageOptions"/> containing the offset and page size for pagination.
        /// </param>
        /// <param name="defaultSort">
        /// An optional default sort expression to apply if sorting is not explicitly specified in the page options.
        /// </param>
        /// <param name="thenBySort">
        /// Additional sort expressions to apply for secondary sorting, in the order they are provided, if applicable.
        /// </param>
        /// <returns>
        /// A <see cref="IQueryable{T}"/> that contains the paginated result based on the provided page options.
        /// </returns>
        public IQueryable<T> ApplyPaging(PageOptions? pageOptions,
            Expression<Func<T, object>>? defaultSort = null,
            params Expression<Func<T, object>>[] thenBySort
        )
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
