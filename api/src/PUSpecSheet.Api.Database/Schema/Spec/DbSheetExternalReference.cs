namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A reference to the sheet from another system.
/// </summary>
public record DbSheetExternalReference
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual ICollection<DbSheetExternalReferenceRevision> Revisions { get; init; } = [];
}
