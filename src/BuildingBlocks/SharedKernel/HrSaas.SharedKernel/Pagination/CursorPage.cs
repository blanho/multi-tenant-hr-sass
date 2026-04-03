namespace HrSaas.SharedKernel.Pagination;

public sealed record CursorPage<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public string? PreviousCursor { get; init; }
    public bool HasNextPage => NextCursor is not null;
    public bool HasPreviousPage => PreviousCursor is not null;
    public int TotalInPage => Items.Count;

    public static CursorPage<T> Empty() => new() { Items = [] };

    public static CursorPage<T> From(
        IReadOnlyList<T> items,
        string? nextCursor,
        string? previousCursor = null) =>
        new() { Items = items, NextCursor = nextCursor, PreviousCursor = previousCursor };

    public CursorPage<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        new()
        {
            Items = Items.Select(mapper).ToList().AsReadOnly(),
            NextCursor = NextCursor,
            PreviousCursor = PreviousCursor
        };
}

public static class CursorEncoder
{
    public static string Encode(Guid id, DateTime createdAt) =>
        Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{id}|{createdAt:O}"));

    public static (Guid Id, DateTime CreatedAt) Decode(string cursor)
    {
        var raw = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
        var parts = raw.Split('|');
        return (Guid.Parse(parts[0]), DateTime.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}
