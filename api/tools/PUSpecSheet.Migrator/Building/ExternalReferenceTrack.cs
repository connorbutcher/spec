using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Migrator.Building;

internal sealed class ExternalReferenceTrack
{
    public ExternalReferenceTrack(DbSheetExternalReference identity)
    {
        Identity = identity;
    }

    public DbSheetExternalReference Identity { get; }

    public int RevisionCount { get; set; }

    public DbSheetExternalReferenceRevision? Last { get; set; }

    public string? Signature { get; set; }
}
