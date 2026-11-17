using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using PUSpecSheet.Migrator.Snapshots;
using PUSpecSheet.Migrator.Verification;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// A hash of everything about a sheet's copies that its revisions are built from: each copy's publish details, what
/// it shows (as <see cref="SnapshotRenderer"/> writes it) and its workflow. A migrated sheet keeps the hash of the
/// copies it was built from, and is built again only when their hash changes.
/// </summary>
internal static class LegacyFingerprint
{
    // Change this when a change to the migrator would build a sheet differently from the same copies, so that every
    // sheet is built again on the next run.
    private const string BuildVersion = "1";

    public static byte[] Of(LegacyCopies legacy)
    {
        var text = new StringBuilder();
        text.Append("build ").Append(BuildVersion).Append('\n');
        foreach (var copy in legacy.Copies.OrderBy(copy => copy.Id))
        {
            text.Append(CultureInfo.InvariantCulture, $"copy {copy.Id} published {copy.PublishedDateTime:O} by {copy.PublishedByUserId}")
                .Append(CultureInfo.InvariantCulture, $" created {copy.CreatedDateTime:O} by {copy.CreatedByUserId}")
                .Append(CultureInfo.InvariantCulture, $" legacy {copy.LegacySpecSheetId} migrated {copy.HasBeenMigrated} change log {SnapshotJson.Text(copy.ChangeLog)}\n");
            foreach (var line in SnapshotRenderer.Render(copy.Snapshot))
            {
                text.Append(line).Append('\n');
            }

            foreach (var step in copy.Workflow)
            {
                text.Append(CultureInfo.InvariantCulture, $"step {step.Id} at {step.StartedDateTime:O} state {step.State}\n");
                foreach (var actor in step.Actors)
                {
                    text.Append(CultureInfo.InvariantCulture, $"  actor {actor.Id} user {actor.UserId} state {actor.State} at {actor.ActionedDateTime:O} comment {SnapshotJson.Text(actor.Comment)}\n");
                }
            }
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString()));
    }
}
