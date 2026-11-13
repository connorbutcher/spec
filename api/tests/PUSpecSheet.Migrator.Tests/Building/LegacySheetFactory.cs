using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Tests.Building;

/// <summary>
/// Builds legacy copies of a small sheet: one table with one section, whose rows each have a single text cell.
/// Every copy gets its own ids, as legacy copies do, so the builder has to match items up itself.
/// </summary>
internal sealed class LegacySheetFactory
{
    public const int TemplateVersionId = 10;
    public const int TemplateSectionId = 20;
    public const int TemplateRowId = 30;
    public const int TemplateCellId = 40;
    public const int UserId = 5;

    private int nextId = 1000;

    /// <summary>A copy whose section has a row for each text, in order. A null text is a row with an empty cell.</summary>
    public LegacyCopy Copy(int versionNumber, DateTime? publishedAtUtc, params string?[] texts)
    {
        var table = new SnapshotTable
        {
            Id = nextId++,
            PublicId = Guid.NewGuid(),
            TableTemplateVersionId = TemplateVersionId,
            Title = "Engine",
            DisplayOrder = 1024,
        };
        var section = new SnapshotSection
        {
            Id = nextId++,
            PublicId = Guid.NewGuid(),
            TableId = table.Id,
            TemplateSectionId = TemplateSectionId,
            DisplayOrder = 1024,
        };

        var rows = new List<SnapshotRow>();
        var cells = new List<SnapshotCell>();
        for (var index = 0; index < texts.Length; index++)
        {
            var row = new SnapshotRow
            {
                Id = nextId++,
                PublicId = Guid.NewGuid(),
                SectionId = section.Id,
                TemplateRowId = TemplateRowId,
                DisplayOrder = (index + 1) * 1024,
            };
            rows.Add(row);
            cells.Add(new SnapshotCell
            {
                Id = nextId++,
                PublicId = Guid.NewGuid(),
                RowId = row.Id,
                TemplateCellId = TemplateCellId,
                Content = texts[index] is { } text ? new CellContent(text: text) : CellContent.Empty,
            });
        }

        return new LegacyCopy
        {
            Id = nextId++,
            VersionNumber = versionNumber,
            PublishedAtUtc = publishedAtUtc,
            UserId = UserId,
            Snapshot = new SheetSnapshot { Tables = [table], Sections = [section], Rows = rows, Cells = cells },
        };
    }
}
