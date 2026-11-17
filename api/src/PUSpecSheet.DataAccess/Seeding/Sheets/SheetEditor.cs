using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Seeding.ReferenceData;
using PUSpecSheet.DataAccess.Sheets;
using PUSpecSheet.DataAccess.Sheets.Editing;
using PUSpecSheet.DataAccess.Sheets.Reading;

namespace PUSpecSheet.DataAccess.Seeding.Sheets;

// Edits one sheet through the same services the API uses, so everything the showcase adds is real: drafts that lock
// what they change, and published versions with history. It keeps the live view the last change returned and finds
// tables, sections and column blocks in it by their template names.
public sealed class SheetEditor
{
    private readonly ShowcaseCellTypeCatalog _cellTypes;
    private readonly ISheetsService _sheets;
    private readonly ISheetTablesService _tables;
    private readonly ISheetSectionsService _sections;
    private readonly ISheetRowsService _rows;
    private readonly ISheetColumnBlocksService _columnBlocks;
    private readonly ISheetCellSettingsService _cellSettings;

    public SheetEditor(
        Sheet sheet,
        ShowcaseCellTypeCatalog cellTypes,
        ISheetsService sheets,
        ISheetTablesService tables,
        ISheetSectionsService sections,
        ISheetRowsService rows,
        ISheetColumnBlocksService columnBlocks,
        ISheetCellSettingsService cellSettings)
    {
        _cellTypes = cellTypes;
        _sheets = sheets;
        _tables = tables;
        _sections = sections;
        _rows = rows;
        _columnBlocks = columnBlocks;
        _cellSettings = cellSettings;
        Sheet = sheet;
    }

    public Sheet Sheet { get; private set; }

    public async Task<int> AddTableAsync(int templateId, string title, CancellationToken cancellationToken)
    {
        var before = Sheet.Tables.Select(table => table.Id).ToHashSet();
        Sheet = await _tables.AddAsync(Sheet.Id, new AddSheetTableRequest { TableTemplateId = templateId }, cancellationToken);
        var tableId = Sheet.Tables.Single(table => !before.Contains(table.Id)).Id;
        Sheet = await _tables.SetTitleAsync(tableId, new SetSheetTableTitleRequest { Title = title }, cancellationToken);
        return tableId;
    }

    public SheetTable Table(int tableId)
    {
        return Sheet.Tables.Single(table => table.Id == tableId);
    }

    // The copy of the addable section called name at index under the table or under parent, adding copies until there
    // is one there. Copies a table starts with count, so the first call for a section with a minimum fills that in
    // rather than adding another.
    public async Task<SheetSection> SectionAsync(int tableId, SheetSection? parent, string name, int index, CancellationToken cancellationToken)
    {
        while (true)
        {
            var copies = Copies(tableId, parent?.Id, name);
            if (copies.Count > index)
            {
                return copies[index];
            }

            var addable = (parent is null ? Table(tableId).AddableSections : FindSection(tableId, parent.Id).AddableSections)
                .Single(section => section.Name == name);
            Sheet = await _sections.AddAsync(tableId, new AddSheetSectionRequest
            {
                TemplateSectionId = addable.TemplateSectionId,
                ParentSheetSectionId = parent?.Id,
            }, cancellationToken);
        }
    }

    public async Task<SheetSection> AddSectionAsync(int tableId, SheetSection? parent, string name, CancellationToken cancellationToken)
    {
        return await SectionAsync(tableId, parent, name, Copies(tableId, parent?.Id, name).Count, cancellationToken);
    }

    public async Task RemoveSectionAsync(SheetSection section, CancellationToken cancellationToken)
    {
        Sheet = await _sections.RemoveAsync(section.Id, cancellationToken);
    }

    public SheetSection FindSection(int tableId, int sectionId)
    {
        return Flatten(Table(tableId).Sections).Single(section => section.Id == sectionId);
    }

    public List<SheetSection> Copies(int tableId, int? parentSectionId, string name)
    {
        var siblings = parentSectionId is null ? Table(tableId).Sections : FindSection(tableId, parentSectionId.Value).Sections;
        return siblings.Where(section => section.Name == name).OrderBy(section => section.DisplayOrder).ToList();
    }

    public async Task<SheetColumnBlock> AddColumnBlockAsync(int tableId, string name, CancellationToken cancellationToken)
    {
        var before = Table(tableId).ColumnBlocks.Select(block => block.Id).ToHashSet();
        var addable = Table(tableId).AddableColumnBlocks.Single(block => block.Name == name);
        Sheet = await _columnBlocks.AddAsync(tableId, new AddSheetColumnBlockRequest
        {
            TemplateColumnBlockId = addable.TemplateColumnBlockId,
        }, cancellationToken);
        return Table(tableId).ColumnBlocks.Single(block => !before.Contains(block.Id));
    }

    public List<SheetColumnBlock> ColumnBlocks(int tableId, string name)
    {
        return Table(tableId).ColumnBlocks.Where(block => block.Name == name).OrderBy(block => block.DisplayOrder).ToList();
    }

    public async Task FillAsync(SheetRow row, params object?[] values)
    {
        await SaveAsync(row, SheetValues.For(Sheet, row, null, _cellTypes, values), CancellationToken.None);
    }

    public List<CellValueRequest> Values(SheetRow row, int? columnBlockId, params object?[] values)
    {
        return SheetValues.For(Sheet, row, columnBlockId, _cellTypes, values);
    }

    public async Task SaveAsync(SheetRow row, List<CellValueRequest> requests, CancellationToken cancellationToken)
    {
        if (requests.Count > 0)
        {
            Sheet = await _rows.SaveValuesAsync(row.Id, new SaveRowValuesRequest { Values = requests }, cancellationToken);
        }
    }

    public async Task ChooseSettingsAsync(SheetRow row, SheetCell cell, CellInstanceSettings settings, CancellationToken cancellationToken)
    {
        Sheet = await _cellSettings.SaveAsync(row.Id, new SaveRowCellSettingsRequest
        {
            Settings = [new CellSettingsRequest { SheetCellId = cell.Id, Settings = settings }],
        }, cancellationToken);
    }

    public async Task PublishAsync(string note, CancellationToken cancellationToken)
    {
        Sheet = await _sheets.PublishAsync(Sheet.Id, note, PublishScope.Mine, cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        Sheet = await _sheets.GetAsync(Sheet.Id, SheetViewPoint.Live, cancellationToken);
    }

    private static IEnumerable<SheetSection> Flatten(IEnumerable<SheetSection> sections)
    {
        foreach (var section in sections)
        {
            yield return section;
            foreach (var child in Flatten(section.Sections))
            {
                yield return child;
            }
        }
    }
}
