using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// The revision of a sheet item that is shown: who wrote it, when, and whether and as which version it was published.
/// </summary>
public record SheetItemRevision
{
    public required int Id { get; init; }

    public required int RevisionNumber { get; init; }

    public required RevisionStatus Status { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public User? CreatedByUser { get; init; }

    public required DateTime LastModifiedDateTime { get; init; }

    public required DateTime? PublishedDateTime { get; init; }

    public required DateTime? SupersededDateTime { get; init; }

    public required int? SheetVersionId { get; init; }

    public required int? SheetVersionNumber { get; init; }
}
