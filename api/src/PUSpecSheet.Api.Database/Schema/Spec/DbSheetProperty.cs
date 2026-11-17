using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A sheet's value of one of its sheet type's properties.
/// </summary>
public record DbSheetProperty
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required int SpecSheetTypePropertyId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbSpecSheetTypeProperty? SpecSheetTypeProperty { get; init; }

    public virtual ICollection<DbSheetPropertyRevision> Revisions { get; init; } = [];
}
