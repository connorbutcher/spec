using Microsoft.Extensions.Logging;

namespace PUSpecSheet.Migrator;

/// <summary>Everything the migrator says.</summary>
internal static partial class MigratorLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "The arguments aren't right.\n{Usage}")]
    public static partial void BadArguments(this ILogger logger, string usage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Migrating {Count} legacy sheet(s); {Mode}.")]
    public static partial void Starting(this ILogger logger, int count, string mode);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sheet {SheetId} ({PublicId}) is already in the target database; skipped.")]
    public static partial void Skipped(this ILogger logger, int sheetId, Guid publicId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sheet {SheetId}: {Warning}")]
    public static partial void SheetWarning(this ILogger logger, int sheetId, string warning);

    [LoggerMessage(Level = LogLevel.Error, Message = "Sheet {SheetId} can't be migrated: {Reason}")]
    public static partial void CantMigrate(this ILogger logger, int sheetId, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Sheet {SheetId} couldn't be saved; nothing was kept.")]
    public static partial void SaveFailed(this ILogger logger, int sheetId, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Sheet {SheetId}, {Which}, doesn't match its legacy copy: {Difference}")]
    public static partial void Mismatch(this ILogger logger, int sheetId, string which, string difference);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sheet {SheetId}: {Copies} copies became {Versions} versions and {Revisions} revisions; {Outcome}.")]
    public static partial void Done(this ILogger logger, int sheetId, int copies, int versions, int revisions, SheetOutcome outcome);

    [LoggerMessage(Level = LogLevel.Information, Message = "Finished: {Migrated} migrated, {CheckedCount} checked and rolled back, {Skipped} skipped, {Failed} failed.")]
    public static partial void Finished(this ILogger logger, int migrated, int checkedCount, int skipped, int failed);
}
