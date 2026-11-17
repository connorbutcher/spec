namespace PUSpecSheet.Migrator;

/// <summary>
/// How bringing one sheet's revisions into step with its copies went.
/// </summary>
internal enum SheetOutcome
{
    /// <summary>
    /// Its copies haven't changed since it was last migrated, so it was left alone.
    /// </summary>
    UpToDate,

    /// <summary>
    /// The sheet's revisions weren't built by the migrator, so they belong to the new app and were left alone.
    /// </summary>
    NotMigrated,

    /// <summary>
    /// Built from its copies, checked against every one of them and kept.
    /// </summary>
    Migrated,

    /// <summary>
    /// Built and checked, then rolled back, as this is a dry run.
    /// </summary>
    Checked,

    /// <summary>
    /// It couldn't be built, or what was built didn't match its copies; nothing changed.
    /// </summary>
    Failed
}
