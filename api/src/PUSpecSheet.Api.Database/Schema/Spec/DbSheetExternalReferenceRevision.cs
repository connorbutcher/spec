namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// What an external reference was in a version. External references only belong to versions, so there are no drafts.
/// </summary>
public record DbSheetExternalReferenceRevision
{
    public required int Id { get; init; }

    public required int SheetExternalReferenceId { get; init; }

    public required int SheetId { get; init; }

    public required int SheetVersionId { get; init; }

    public required int RevisionNumber { get; init; }

    public required int ExternalId { get; init; }

    public required string SystemName { get; init; }

    public required string OwnerName { get; init; }

    public required string Url { get; init; }

    public required string Type { get; init; }

    public required string? Grouping { get; init; }

    public required string? SystemIconUrl { get; init; }

    public required string? Description { get; init; }

    public required bool IsDeleted { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required DateTime PublishedDateTime { get; init; }

    public required DateTime? SupersededDateTime { get; set; }

    public byte[] RowVersion { get; init; } = [];

    public virtual DbSheetVersion? SheetVersion { get; set; }
}
