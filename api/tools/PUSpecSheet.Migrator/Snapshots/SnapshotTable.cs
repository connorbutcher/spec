using PUSpecSheet.Abstractions.Domain.Properties.Table;

namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// A table of a sheet snapshot. <see cref="Id"/> is its id where it was read from: a SpecSheetTables row, or a
/// spec.SheetTables row.
/// </summary>
internal sealed record SnapshotTable
{
    public required int Id { get; init; }

    public required Guid ReferenceId { get; init; }

    public required int TableTemplateId { get; init; }

    public required string Title { get; init; }

    public required int DisplayOrder { get; init; }

    public required ITableProperties? ConfigurationOverride { get; init; }

    public required IReadOnlyList<SnapshotWidgetValue> WidgetValues { get; init; }

    public required IReadOnlyList<SnapshotNote> Notes { get; init; }
}
