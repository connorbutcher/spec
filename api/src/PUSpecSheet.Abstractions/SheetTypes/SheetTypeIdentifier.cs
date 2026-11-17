namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>
/// A value that tells one sheet of a sheet type from another within a phase, such as an engine
/// specification's serial number. <see cref="Key"/> names it in the published API's filters.
/// </summary>
public sealed record SheetTypeIdentifier
{
    public required int Id { get; init; }

    public required string Key { get; init; }

    public required string Name { get; init; }

    public required SheetIdentifierKind Kind { get; init; }

    public required bool IsRequired { get; init; }

    public required int DisplayOrder { get; init; }

    /// <summary>The choices of an option identifier; empty for the other kinds.</summary>
    public required IReadOnlyList<string> Options { get; init; }
}
