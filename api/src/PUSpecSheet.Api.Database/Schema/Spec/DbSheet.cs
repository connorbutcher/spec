using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A sheet: one per phase and sheet type. What it holds is in its items' revisions, and each publish of it is a <see cref="DbSheetVersion"/>. A sheet the migrator built from the copied sheets has the hash of what it was built from in <see cref="LegacySourceHash"/>.
/// </summary>
public record DbSheet
{
    public required int Id { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required byte[]? LegacySourceHash { get; set; }

    public required DateTime? MigratedDateTime { get; set; }

    public byte[] RowVersion { get; init; } = [];

    public virtual DbSpecSheetPhase? SpecSheetPhase { get; init; }

    public virtual DbSpecSheetType? SpecSheetType { get; init; }

    public virtual ICollection<DbSheetVersion> Versions { get; init; } = [];

    public virtual ICollection<DbSheetTable> Tables { get; init; } = [];

    public virtual ICollection<DbSheetProperty> Properties { get; init; } = [];

    public virtual ICollection<DbSheetLinkedTable> LinkedTables { get; init; } = [];

    public virtual ICollection<DbSheetExternalReference> ExternalReferences { get; init; } = [];
}
