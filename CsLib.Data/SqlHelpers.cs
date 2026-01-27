using System.Text;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Grad.CsLib.Data;

/// <summary>
/// Provides utility methods for SQL query construction, sorting, and filtering.
/// </summary>
/// <remarks>
/// Only use these in scenarios where the query is very dynamic. Otherwise, it's
/// recommended to write complete SQL queries directly for better clarity and maintainability.
/// </remarks>
public static class SqlHelpers
{
    /// <summary>
    /// Builds an ORDER BY clause based on a simplified string representation of sorting options,
    /// typically sent from a web client.
    /// </summary>
    /// <param name="entity">EF Core entity type for normalizing columns.</param>
    /// <param name="mapping">A dictionary mapping property names to entity field names.</param>
    /// <param name="defaultSort">The default sorting order if no specific sorting is provided.</param>
    /// <param name="pageOptions">The page options containing sorting information.</param>
    /// <returns>The constructed ORDER BY clause.</returns>
    /// <exception cref="InvalidParameterException">Thrown if the sorting parameter is invalid.</exception>
    public static string BuildOrderBy<T>(T entity, Dictionary<string, string> mapping, string defaultSort, PageOptions? pageOptions) where T : IEntityType
    {
        if (string.IsNullOrWhiteSpace(pageOptions?.SortBy))
        {
            return $"ORDER BY {defaultSort}";
        }
        
        var parts = pageOptions.SortBy.Split(',')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();

        if (parts.Length == 0)
            throw new InvalidParameterException("No valid columns specified in column path.");

        var orderParts = new List<string>(parts.Length);
        foreach (var part in parts)
        {
            var isDescending = part.StartsWith('-');
            var key = isDescending ? part[1..] : part;

            if (!mapping.TryGetValue(key, out var mappedColumn))
                throw new InvalidParameterException($"Invalid field in column path '{key}'.");
            
            var sqlColumn = entity.FindProperty(mappedColumn)?.GetColumnName() ?? mappedColumn;

            orderParts.Add(isDescending ? $"{sqlColumn} DESC" : sqlColumn);
        }

        return "ORDER BY " + string.Join(", ", orderParts);
    }

    /// <summary>
    /// Builds a paging clause based on the provided page options.
    /// </summary>
    /// <param name="pageOptions">The page options to use for paging.</param>
    /// <param name="parameters">The dynamic parameters to add paging parameters to.</param>
    /// <returns>The SQL paging clause or an empty string if no paging options are provided.</returns>
    public static string BuildPaging(PageOptions? pageOptions, DynamicParameters parameters)
    {
        if (pageOptions == null)
            return string.Empty;

        parameters.Add("PageOffset", pageOptions.PageOffset);
        parameters.Add("PageSize", pageOptions.PageSize);
        return "OFFSET @PageOffset ROWS FETCH NEXT @PageSize ROWS ONLY";
    }

    /// <summary>
    /// Appends filter conditions to a SQL WHERE clause based on the specified column-value pairs.
    /// Handles string, boolean, and collection-based filters dynamically, adding the appropriate SQL syntax.
    /// </summary>
    /// <param name="where">The StringBuilder object to which the filter conditions will be appended.</param>
    /// <param name="parameters">The dynamic parameters object for managing SQL parameterized values.</param>
    /// <param name="filters">A collection of tuples representing the column names and corresponding filter values.</param>
    /// <param name="booleanNullCheck">
    /// Indicates whether to add IS NULL/IS NOT NULL conditions for boolean values when they are null.
    /// Defaults to true.
    /// </param>
    public static void AppendFilters(StringBuilder where,
        DynamicParameters parameters,
        IEnumerable<(string Column, object? Value)> filters,
        bool booleanNullCheck = true
    )
    {
        foreach (var (column, value) in filters)
        {
            if (value is null) continue;

            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                var paramName = column.Split('.').Last();
                where.AppendLine($"  AND {column} = @{paramName}");
                parameters.Add(paramName, s);
            }
            else if (value is bool b && booleanNullCheck)
            {
                where.AppendLine($"  AND {column} IS {(b ? "NOT " : "")}NULL");
            }
            else if (value is System.Collections.ICollection { Count: > 0 } c)
            {
                var paramName = column.Split('.').Last();
                where.AppendLine($"  AND {column} IN @{paramName}");
                parameters.Add(paramName, c);
            }
        }
    }
}