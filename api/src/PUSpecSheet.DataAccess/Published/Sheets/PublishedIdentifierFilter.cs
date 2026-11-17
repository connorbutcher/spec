namespace PUSpecSheet.DataAccess.Published.Sheets;

public sealed record PublishedIdentifierFilter
{
    public required int IdentifierId { get; init; }

    public string? Value { get; init; }
}
