namespace PUSpecSheet.Migrator;

/// <summary>How migrating one legacy sheet went.</summary>
internal enum SheetOutcome
{
    /// <summary>The sheet is already in the target database, so it was left alone.</summary>
    Skipped,

    /// <summary>Migrated, checked against every copy and kept.</summary>
    Migrated,

    /// <summary>Migrated and checked against every copy, then rolled back, as this is a dry run.</summary>
    Checked,

    /// <summary>It couldn't be migrated, or the result didn't match its copies; nothing was kept.</summary>
    Failed,
}
