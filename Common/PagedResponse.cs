namespace ApiBase.Common;

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; init; }
    public PaginationMeta Meta { get; init; }

    public PagedResponse(IEnumerable<T> items, int totalItems, PaginationParams pagination)
    {
        var totalPages = (int)Math.Ceiling((double)totalItems / pagination.PageSize);

        Items = items;
        Meta = new PaginationMeta(
            Page: pagination.Page,
            PageSize: pagination.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages,
            HasNextPage: pagination.Page < totalPages,
            HasPreviousPage: pagination.Page > 1
        );
    }
}
