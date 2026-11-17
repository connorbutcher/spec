namespace PUSpecSheet.Abstractions.Published;

/// <summary>A row of a published section, with its cells that hold a value. A row that holds none is left out.</summary>
public sealed record PublishedRowDto
{
    public required Guid Id { get; init; }

    public required IReadOnlyList<PublishedCellDto> Cells { get; init; }
}
