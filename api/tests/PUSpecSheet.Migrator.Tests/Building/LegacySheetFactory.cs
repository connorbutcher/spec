using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Tests.Building;

// Builds copies of a small sheet: one table with one group, whose rows each have a single text cell and are numbered
// from 1, as the app numbers them. Every copy gets its own ids, as copies do, but keeps each item's ReferenceID, as a
// copy made from another does. A row's ReferenceID comes from its text, ignoring case, so "b" and "B" are one row
// whose text changed.
internal sealed class LegacySheetFactory
{
    public const int TableTemplateId = 10;
    public const int TableGroupTemplateId = 20;
    public const int TableRowTemplateId = 30;
    public const int TableCellTemplateId = 40;
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

    public LegacyCopy Copy(DateTime? publishedDateTime, params string[] texts)
    {
        return Copy(publishedDateTime, snapshot => snapshot, texts);
    }

    public LegacyCopy Copy(DateTime? publishedDateTime, Func<SheetSnapshot, SheetSnapshot> change, params string[] texts)
    {
        var table = new SnapshotTable
        {
            Id = _nextId++,
            ReferenceId = ReferenceOf("table"),
            TableTemplateId = TableTemplateId,
            Title = "Engine",
            DisplayOrder = 1,
            ConfigurationOverride = null,
            WidgetValues = [],
            Notes = []
        };
        var group = new SnapshotGroup
        {
            Id = _nextId++,
            ReferenceId = ReferenceOf("group"),
            TableId = table.Id,
            TableGroupTemplateId = TableGroupTemplateId,
            DisplayOrder = 1,
            WidgetValues = [],
            LinkedGroupReferenceId = null
        };

        var rows = new List<SnapshotRow>();
        var cells = new List<SnapshotCell>();
        for (var index = 0; index < texts.Length; index++)
        {
            var row = new SnapshotRow
            {
                Id = _nextId++,
                ReferenceId = ReferenceOf("row " + texts[index]),
                GroupId = group.Id,
                TableRowTemplateId = TableRowTemplateId,
                DisplayOrder = index + 1
            };
            rows.Add(row);
            cells.Add(new SnapshotCell
            {
                Id = _nextId++,
                ReferenceId = ReferenceOf("cell " + texts[index]),
                RowId = row.Id,
                TableCellTemplateId = TableCellTemplateId,
                Content = CellContent.Empty with { Text = texts[index] }
            });
        }

        return new LegacyCopy
        {
            Id = _nextId++,
            PublishedDateTime = publishedDateTime,
            PublishedByUserId = publishedDateTime is null ? null : UserId,
            CreatedByUserId = UserId,
            CreatedDateTime = publishedDateTime ?? DateTime.MaxValue,
            ChangeLog = null,
            LegacySpecSheetId = null,
            HasBeenMigrated = false,
            Snapshot = change(new SheetSnapshot
            {
                Tables = [table],
                Groups = [group],
                Rows = rows,
                Cells = cells,
                Properties = new Dictionary<int, string?>(),
                LinkedTables = [],
                ExternalReferences = publishedDateTime is null ? null : []
            }),
            Workflow = []
        };
    }
}
