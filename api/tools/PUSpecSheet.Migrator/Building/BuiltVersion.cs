using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// What the migrated sheet must show once saved: <see cref="Expected"/>, the copy, as of <see cref="Moment"/>, or with
/// its drafts when that is null (the working copy).
/// </summary>
internal sealed record BuiltVersion
{
    public required int VersionNumber { get; init; }

    public required DateTime? Moment { get; init; }

    public required SheetSnapshot Expected { get; init; }
}
