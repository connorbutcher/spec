using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Verification;

/// <summary>Compares what a legacy copy showed with what the migrated sheet shows at the same point.</summary>
internal static class SheetVerifier
{
    /// <summary>Null when the two show the same sheet; otherwise where they first differ, for a person to read.</summary>
    public static string? Difference(SheetSnapshot expected, SheetSnapshot actual)
    {
        var want = SnapshotRenderer.Render(expected);
        var got = SnapshotRenderer.Render(actual);

        for (var line = 0; line < Math.Max(want.Count, got.Count); line++)
        {
            var wanted = line < want.Count ? want[line] : "(nothing more)";
            var found = line < got.Count ? got[line] : "(nothing more)";
            if (!string.Equals(wanted, found, StringComparison.Ordinal))
            {
                return $"line {line + 1} of {want.Count}: the legacy copy has\n    {wanted.Trim()}\n  but the migrated sheet has\n    {found.Trim()}";
            }
        }

        return null;
    }
}
