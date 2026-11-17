namespace PUSpecSheet.DataAccess.Seeding.Sheets;

public sealed record SheetTarget
{
    public required ShowcaseContext Context { get; init; }

    public required int SheetTypeId { get; init; }

    public required IReadOnlyDictionary<string, int> IdentifierIds { get; init; }
}
