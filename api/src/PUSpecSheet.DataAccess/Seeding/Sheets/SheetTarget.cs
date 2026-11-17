namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>Where the showcase creates sheets of a sheet type with identifiers: the showcase, the sheet type and its identifier ids by name.</summary>
public sealed record SheetTarget
{
    public SheetTarget(ShowcaseContext context, int sheetTypeId, IReadOnlyDictionary<string, int> identifierIds)
    {
        this.Context = context;
        this.SheetTypeId = sheetTypeId;
        this.IdentifierIds = identifierIds;
    }

    public ShowcaseContext Context { get; init; }

    public int SheetTypeId { get; init; }

    public IReadOnlyDictionary<string, int> IdentifierIds { get; init; }
}
