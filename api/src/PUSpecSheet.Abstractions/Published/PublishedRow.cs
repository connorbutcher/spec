namespace PUSpecSheet.Abstractions.Published;

/// <summary>A row of a published section, with its cells that hold a value. A row that holds none is left out.</summary>
public sealed record PublishedRow
{
    public required Guid Id { get; init; }

    public required IReadOnlyList<PublishedCell> Cells { get; init; }
}
