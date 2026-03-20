namespace ECommerceCenter.Application.Common.Pagination;

/// <summary>
/// Generic paginated result wrapper shared across all modules.
/// </summary>
public record PaginatedList<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
