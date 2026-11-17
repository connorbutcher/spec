using Microsoft.Extensions.Logging;

namespace PUSpecSheet.Migrator;

/// <summary>Everything the migrator says.</summary>
internal static partial class MigratorLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "The arguments aren't right.\n{Usage}")]
    public static partial void BadArguments(this ILogger logger, string usage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Migrating {Count} legacy sheet(s); {Mode}.")]
    public static partial void Starting(this ILogger logger, int count, string mode);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Sheet} is already in the target database; skipped.")]
    public static partial void Skipped(this ILogger logger, string sheet);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Sheet}: {Warning}")]
    public static partial void SheetWarning(this ILogger logger, string sheet, string warning);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Sheet} can't be migrated: {Reason}")]
    public static partial void CantMigrate(this ILogger logger, string sheet, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Sheet} couldn't be saved; nothing was kept.")]
    public static partial void SaveFailed(this ILogger logger, string sheet, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Sheet}, {Which}, doesn't match its legacy copy: {Difference}")]
    public static partial void Mismatch(this ILogger logger, string sheet, string which, string difference);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Sheet}: {Copies} copies became {Versions} versions, {Revisions} revisions and {Submissions} submissions; {Outcome}.")]
    public static partial void Done(this ILogger logger, string sheet, int copies, int versions, int revisions, int submissions, SheetOutcome outcome);

    [LoggerMessage(Level = LogLevel.Error, Message = "Legacy user {User} isn't in the target database; add every legacy user, with the same id, before migrating.")]
    public static partial void UserMissing(this ILogger logger, string user);

    [LoggerMessage(Level = LogLevel.Information, Message = "Legacy access: user {UserId} had {Access}.")]
    public static partial void UserAccess(this ILogger logger, int userId, string access);

    [LoggerMessage(Level = LogLevel.Information, Message = "Legacy access: sheet type {SheetTypeId} required permission {Permission}.")]
    public static partial void SheetTypeAccess(this ILogger logger, int sheetTypeId, string permission);

    [LoggerMessage(Level = LogLevel.Information, Message = "Reference data: added {Count} widget type, template widget and sheet type property rows.")]
    public static partial void ReferenceDataAdded(this ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "The reference data couldn't be migrated: {Reason}")]
    public static partial void ReferenceDataFailed(this ILogger logger, string reason);

    [LoggerMessage(Level = LogLevel.Information, Message = "Finished: {Migrated} migrated, {CheckedCount} checked and rolled back, {Skipped} skipped, {Failed} failed.")]
    public static partial void Finished(this ILogger logger, int migrated, int checkedCount, int skipped, int failed);
}
