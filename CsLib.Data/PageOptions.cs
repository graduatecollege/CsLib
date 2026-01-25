namespace Grad.CsLib.Data;

public record PageOptions(
    int PageOffset = 0,
    int PageSize = 100,
    string? SortBy = null
);