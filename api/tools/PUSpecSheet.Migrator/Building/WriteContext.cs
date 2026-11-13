using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// What every revision written for one copy shares: the version it is published in (null for the working copy's
/// drafts), when, and who by.
/// </summary>
internal sealed record WriteContext(DbSheetVersion? Version, DateTime At, int UserId);
