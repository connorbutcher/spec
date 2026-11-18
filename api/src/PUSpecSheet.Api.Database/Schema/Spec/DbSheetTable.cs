using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A table of a sheet. <see cref="ReferenceId"/> is the ReferenceID it has in every SpecSheetTables copy.
/// </summary>
public record DbSheetTable
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required int TableTemplateId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbTableTemplate? TableTemplate { get; init; }

    public virtual ICollection<DbSheetTableRevision> Revisions { get; init; } = [];

    public virtual ICollection<DbSheetTableGroup> Groups { get; init; } = [];

    public virtual ICollection<DbSheetTableWidget> Widgets { get; init; } = [];

    public virtual ICollection<DbSheetTableNote> Notes { get; init; } = [];
}
