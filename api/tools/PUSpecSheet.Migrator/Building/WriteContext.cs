using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// What every revision written for one copy shares: the version it is published in (null for the working copy's
/// drafts), when, and who by.
/// </summary>
internal sealed record WriteContext
{
    public required DbSheetVersion? Version { get; init; }

    public required DateTime At { get; init; }

    public required int UserId { get; init; }

    public RevisionStatus Status => Version is null ? RevisionStatus.Draft : RevisionStatus.Published;

    public DateTime? PublishedDateTime => Version is null ? null : At;
}
