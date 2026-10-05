namespace PUSpecSheet.Contracts.Published;

/// <summary>A row of a published section, with its cells that hold a value. A row that holds none is left out.</summary>
public sealed record PublishedRowDto(
    Guid Id,
    IReadOnlyList<PublishedCellDto> Cells);
