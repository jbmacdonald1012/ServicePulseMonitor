namespace ServicePulseMonitor.Common;

/// <summary>A page of results from a paginated query.</summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public record PagedResult<T>
{
    /// <summary>The items on this page.</summary>
    public IEnumerable<T> Items { get; init; } = [];

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>The current 1-based page number.</summary>
    public int PageNumber { get; init; }

    /// <summary>Maximum number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Total number of pages given <see cref="TotalCount"/> and <see cref="PageSize"/>.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary><c>true</c> if there is a page before this one.</summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary><c>true</c> if there is a page after this one.</summary>
    public bool HasNext => PageNumber < TotalPages;
}
