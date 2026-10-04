namespace PUSpecSheet.Contracts.Published;

/// <summary>A row of a published section. Only its cells that hold a value are listed.</summary>
public sealed record PublishedRowDto(
    Guid Id,
    IReadOnlyList<PublishedCellDto> Cells);
