using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Tests.Building;

// Builds legacy copies of a small sheet: one table with one section, whose rows each have a single text cell. Every
// copy gets its own ids, as legacy copies do, but keeps each item's ReferenceID, as a copy made from another does. A
// row's ReferenceID comes from its text, ignoring case, so "b" and "B" are one row whose text changed.
internal sealed class LegacySheetFactory
{
    public const int TemplateVersionId = 10;
    public const int TemplateSectionId = 20;
    public const int TemplateRowId = 30;
    public const int TemplateCellId = 40;
    public const int UserId = 5;

    private readonly Dictionary<string, Guid> _references = [];
    private int _nextId = 1000;

    public Guid ReferenceOf(string name)
    {
        var key = name.ToUpperInvariant();
        if (!_references.TryGetValue(key, out var reference))
        {
            reference = Guid.NewGuid();
            _references[key] = reference;
        }

        return reference;
    }

    public LegacyCopy Copy(DateTime? publishedAtUtc, params string[] texts)
    {
        return Copy(publishedAtUtc, snapshot => snapshot, texts);
    }

    public LegacyCopy Copy(DateTime? publishedAtUtc, Func<SheetSnapshot, SheetSnapshot> change, params string[] texts)
    {
        var table = new SnapshotTable
        {
            Id = _nextId++,
            PublicId = ReferenceOf("table"),
            TableTemplateVersionId = TemplateVersionId,
            Title = "Engine",
            DisplayOrder = 1024,
        };
        var section = new SnapshotSection
        {
            Id = _nextId++,
            PublicId = ReferenceOf("section"),
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
                Id = _nextId++,
                PublicId = ReferenceOf("row " + texts[index]),
                SectionId = section.Id,
                TemplateRowId = TemplateRowId,
                DisplayOrder = (index + 1) * 1024,
            };
            rows.Add(row);
            cells.Add(new SnapshotCell
            {
                Id = _nextId++,
                PublicId = ReferenceOf("cell " + texts[index]),
                RowId = row.Id,
                TemplateCellId = TemplateCellId,
                Content = new CellContent(text: texts[index]),
            });
        }

        return new LegacyCopy
        {
            Id = _nextId++,
            PublishedAtUtc = publishedAtUtc,
            PublishedByUserId = publishedAtUtc is null ? null : UserId,
            CreatedByUserId = UserId,
            CreatedAtUtc = publishedAtUtc ?? DateTime.MaxValue,
            Snapshot = change(new SheetSnapshot
            {
                Tables = [table],
                Sections = [section],
                Rows = rows,
                Cells = cells,
                ExternalReferences = publishedAtUtc is null ? null : [],
            }),
        };
    }
}
