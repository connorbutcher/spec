using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.Sheets.Editing;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Edits one sheet through the same services the API uses, so everything the showcase adds is real: drafts
/// that lock what they change, and published versions with history. It keeps the live view the last change
/// returned and finds tables, sections and column blocks in it by their template names.
/// </summary>
public sealed class SheetEditor(
    SheetDto sheet,
    ShowcaseCellTypeCatalog cellTypes,
    ISheetService sheets,
    ISheetTableService tables,
    ISheetSectionService sections,
    ISheetRowService rows,
    ISheetColumnBlockService columnBlocks,
    ISheetCellSettingsService cellSettings)
{
    /// <summary>The sheet as the current user last saw it.</summary>
    public SheetDto Sheet { get; private set; } = sheet;

    /// <summary>Adds a table from the latest version of a template, gives it a title and returns its id.</summary>
    public async Task<int> AddTableAsync(int templateId, string title, CancellationToken cancellationToken)
    {
        var before = Sheet.Tables.Select(table => table.Id).ToHashSet();
        Sheet = await tables.AddAsync(Sheet.Id, new AddSheetTableRequest(templateId), cancellationToken);
        var tableId = Sheet.Tables.Single(table => !before.Contains(table.Id)).Id;
        Sheet = await tables.SetTitleAsync(tableId, new UpdateSheetTableRequest(title), cancellationToken);
        return tableId;
    }

    public SheetTableDto Table(int tableId)
    {
        return Sheet.Tables.Single(table => table.Id == tableId);
    }

    /// <summary>
    /// The copy of the addable section called <paramref name="name"/> at <paramref name="index"/> under the
    /// table or under <paramref name="parent"/>, adding copies until there is one there. Copies a table starts
    /// with count, so the first call for a section with a minimum fills that in rather than adding another.
    /// </summary>
    public async Task<SheetSectionDto> SectionAsync(int tableId, SheetSectionDto? parent, string name, int index, CancellationToken cancellationToken)
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
            Sheet = await sections.AddAsync(tableId, new AddSheetSectionRequest(addable.TemplateSectionId, parent?.Id), cancellationToken);
        }
    }

    /// <summary>Adds one more copy of the addable section called <paramref name="name"/> and returns it.</summary>
    public async Task<SheetSectionDto> AddSectionAsync(int tableId, SheetSectionDto? parent, string name, CancellationToken cancellationToken)
    {
        return await SectionAsync(tableId, parent, name, Copies(tableId, parent?.Id, name).Count, cancellationToken);
    }

    public async Task RemoveSectionAsync(SheetSectionDto section, CancellationToken cancellationToken)
    {
        Sheet = await sections.RemoveAsync(section.Id, cancellationToken);
    }

    /// <summary>The section <paramref name="sectionId"/> as it is now.</summary>
    public SheetSectionDto FindSection(int tableId, int sectionId)
    {
        return Flatten(Table(tableId).Sections).Single(section => section.Id == sectionId);
    }

    /// <summary>The copies of a section under the table or a parent section, top to bottom.</summary>
    public List<SheetSectionDto> Copies(int tableId, int? parentSectionId, string name)
    {
        var siblings = parentSectionId is null ? Table(tableId).Sections : FindSection(tableId, parentSectionId.Value).Sections;
        return siblings.Where(section => section.Name == name).OrderBy(section => section.DisplayOrder).ToList();
    }

    /// <summary>Adds a copy of the column block called <paramref name="name"/> and returns it.</summary>
    public async Task<SheetColumnBlockDto> AddColumnBlockAsync(int tableId, string name, CancellationToken cancellationToken)
    {
        var before = Table(tableId).ColumnBlocks.Select(block => block.Id).ToHashSet();
        var addable = Table(tableId).AddableColumnBlocks.Single(block => block.Name == name);
        Sheet = await columnBlocks.AddAsync(tableId, new AddSheetColumnBlockRequest(addable.TemplateColumnBlockId), cancellationToken);
        return Table(tableId).ColumnBlocks.Single(block => !before.Contains(block.Id));
    }

    /// <summary>The copies of the column block called <paramref name="name"/>, left to right.</summary>
    public List<SheetColumnBlockDto> ColumnBlocks(int tableId, string name)
    {
        return Table(tableId).ColumnBlocks.Where(block => block.Name == name).OrderBy(block => block.DisplayOrder).ToList();
    }

    /// <summary>Fills the row's own cells, left to right, with <paramref name="values"/>.</summary>
    public async Task FillAsync(SheetRowDto row, params object?[] values)
    {
        await SaveAsync(row, SheetValues.For(Sheet, row, null, cellTypes, values), CancellationToken.None);
    }

    /// <summary>The requests that fill a row's own cells, or its cells in one column block copy.</summary>
    public List<CellValueRequest> Values(SheetRowDto row, int? columnBlockId, params object?[] values)
    {
        return SheetValues.For(Sheet, row, columnBlockId, cellTypes, values);
    }

    public async Task SaveAsync(SheetRowDto row, List<CellValueRequest> requests, CancellationToken cancellationToken)
    {
        if (requests.Count > 0)
        {
            Sheet = await rows.SaveValuesAsync(row.Id, new SaveRowValuesRequest(requests), cancellationToken);
        }
    }

    /// <summary>Chooses a cell's settings on the sheet: where a linked dropdown takes its choices from, for example.</summary>
    public async Task ChooseSettingsAsync(SheetRowDto row, SheetCellDto cell, CellInstanceSettings settings, CancellationToken cancellationToken)
    {
        Sheet = await cellSettings.SaveAsync(row.Id, new SaveRowCellSettingsRequest([new CellSettingsRequest(cell.Id, settings)]), cancellationToken);
    }

    /// <summary>Publishes the current user's drafts as the sheet's next version.</summary>
    public async Task PublishAsync(string note, CancellationToken cancellationToken)
    {
        Sheet = await sheets.PublishAsync(Sheet.Id, new PublishSheetRequest(note), cancellationToken);
    }

    /// <summary>Reads the sheet again, as whoever the current user now is.</summary>
    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        Sheet = await sheets.GetAsync(Sheet.Id, SheetViewPoint.Live, cancellationToken);
    }

    private static IEnumerable<SheetSectionDto> Flatten(IEnumerable<SheetSectionDto> sections)
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
