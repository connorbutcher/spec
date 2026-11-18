namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// An external reference as of the version that last changed it. These are only revised when a version is published, so
/// they have no drafts and no status.
/// </summary>
public record SheetExternalReference
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required int RevisionNumber { get; init; }

    public required int SheetVersionId { get; init; }

    public required DateTime PublishedDateTime { get; init; }

    public required DateTime? SupersededDateTime { get; init; }

    public required int ExternalId { get; init; }

    public required string SystemName { get; init; }

    public required string OwnerName { get; init; }

    public required string Url { get; init; }

    public required string Type { get; init; }

    public required string? Grouping { get; init; }

    public required string? SystemIconUrl { get; init; }

    public required string? Description { get; init; }
}
