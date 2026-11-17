using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// What every revision written for one copy shares: the version it is published in (null for the working copy's
/// drafts), when, and who by.
/// </summary>
internal sealed record WriteContext
{
    public DbSheetVersion? Version { get; init; }

    public required DateTime At { get; init; }

    public required int UserId { get; init; }
}
