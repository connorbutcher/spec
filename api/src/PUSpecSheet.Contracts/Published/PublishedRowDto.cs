namespace PUSpecSheet.Contracts.Published;

/// <summary>A row of a published section, with its cells that hold a value. A row that holds none is left out.</summary>
public sealed record PublishedRowDto
{
    public PublishedRowDto(Guid id, IReadOnlyList<PublishedCellDto> cells)
    {
        this.Id = id;
        this.Cells = cells;
    }

    public Guid Id { get; init; }

    public IReadOnlyList<PublishedCellDto> Cells { get; init; }
}
