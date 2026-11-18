using Microsoft.Extensions.Logging;

namespace PUSpecSheet.Migrator;

/// <summary>
/// Everything the migrator says.
/// </summary>
internal static partial class MigratorLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "{Error}\n{Usage}")]
    public static partial void BadOptions(this ILogger logger, string error, string usage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Checking {Count} sheet(s); {Mode}.")]
    public static partial void Starting(this ILogger logger, int count, string mode);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Sheet}: its copies haven't changed since it was migrated.")]
    public static partial void UpToDate(this ILogger logger, string sheet);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Sheet}: its revisions weren't built by the migrator, so it is left alone.")]
    public static partial void NotMigrated(this ILogger logger, string sheet);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Sheet}: {Warning}")]
    public static partial void SheetWarning(this ILogger logger, string sheet, string warning);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Sheet} can't be migrated: {Reason}")]
    public static partial void CantMigrate(this ILogger logger, string sheet, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Sheet} couldn't be saved; nothing changed.")]
    public static partial void SaveFailed(this ILogger logger, string sheet, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Sheet}, {Which}, doesn't match its copy: {Difference}")]
    public static partial void Mismatch(this ILogger logger, string sheet, string which, string difference);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Sheet}: {Copies} copies became {Versions} versions, {Revisions} revisions and {Submissions} submissions; {Outcome}.")]
    public static partial void Done(this ILogger logger, string sheet, int copies, int versions, int revisions, int submissions, SheetOutcome outcome);

    [LoggerMessage(Level = LogLevel.Information, Message = "Finished: {Migrated} migrated, {CheckedCount} checked and rolled back, {UpToDate} up to date, {NotMigrated} left alone, {Failed} failed.")]
    public static partial void Finished(this ILogger logger, int migrated, int checkedCount, int upToDate, int notMigrated, int failed);
}
