namespace PUSpecSheet.DataAccess.Published.Sheets;

/// <summary>
/// One identifier a caller narrowed the published sheets by, with the value in its stored form; a null
/// value asks for the sheets with no value for the identifier.
/// </summary>
public sealed record PublishedIdentifierFilter
{
    public required int IdentifierId { get; init; }

    public string? Value { get; init; }
}
