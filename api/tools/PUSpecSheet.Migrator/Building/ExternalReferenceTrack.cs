using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// One external reference's history while it is being built.
/// </summary>
internal sealed class ExternalReferenceTrack
{
    public ExternalReferenceTrack(DbSheetExternalReference item)
    {
        Item = item;
    }

    public DbSheetExternalReference Item { get; }

    public int RevisionCount { get; set; }

    public DbSheetExternalReferenceRevision? Last { get; set; }

    public string? Signature { get; set; }
}
