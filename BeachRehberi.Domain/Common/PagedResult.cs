namespace BeachRehberi.Domain.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = new List<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    // Parameterless constructor for serialization
    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
        => new(items, totalCount, pageNumber, pageSize);

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 20)
        => new(new List<T>(), 0, pageNumber, pageSize);
}
