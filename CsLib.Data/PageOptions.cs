namespace Grad.CsLib.Data;

/// <summary>
/// Page options for database queries.
/// </summary>
/// <param name="PageOffset">The offset of the page to retrieve.</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="SortBy">The field to sort by.</param>
public record PageOptions(
    int PageOffset = 0,
    int PageSize = 100,
    string? SortBy = null
);