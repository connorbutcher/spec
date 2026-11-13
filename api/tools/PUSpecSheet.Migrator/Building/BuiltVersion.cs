using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// What the migrated sheet must show once saved: <see cref="Expected"/>, the legacy copy, as of
/// <see cref="MomentUtc"/>, or with its drafts when that is null (the working copy).
/// </summary>
internal sealed record BuiltVersion(int VersionNumber, DateTime? MomentUtc, SheetSnapshot Expected);
