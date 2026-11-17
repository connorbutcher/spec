namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>Where the showcase creates sheets of a sheet type with identifiers: the showcase, the sheet type and its identifier ids by name.</summary>
public sealed record SheetTarget
{
    public required ShowcaseContext Context { get; init; }

    public required int SheetTypeId { get; init; }

    public required IReadOnlyDictionary<string, int> IdentifierIds { get; init; }
}
