namespace Peerly.Core.Pagination;

public sealed record PaginationInfo
{
    public required int Offset { get; init; }
    public required int PageSize { get; init; }

    public static PaginationInfo SinglePage { get; } = new()
    {
        Offset = 0,
        PageSize = int.MaxValue
    };

    public static PaginationInfo SingleItem { get; } = new()
    {
        Offset = 0,
        PageSize = 1
    };
}
