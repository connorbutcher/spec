using PUSpecSheet.DataAccess.Published.Sheets;

namespace PUSpecSheet.DataAccess.Published;

/// <summary>
/// The tables, sections and column blocks of a sheet at one version, and which of them were on the sheet.
/// Removing a table or section removes everything inside it, so a section is only on the sheet when its
/// table and every section above it are too.
/// </summary>
public sealed class PublishedStructure
{
    private readonly Dictionary<int, PublishedTableRecord> _tables;
    private readonly Dictionary<int, PublishedSectionRecord> _sections;
    private readonly Dictionary<int, PublishedColumnBlockRecord> _columnBlocks;

    public PublishedStructure(
        IEnumerable<PublishedTableRecord> tables,
        IEnumerable<PublishedSectionRecord> sections,
        IEnumerable<PublishedColumnBlockRecord> columnBlocks)
    {
        _tables = tables.Where(table => !table.IsDeleted).ToDictionary(table => table.Id);

        var candidates = sections.Where(section => !section.IsDeleted).ToDictionary(section => section.Id);
        _sections = candidates.Values
            .Where(section => IsOnSheet(section, candidates))
            .ToDictionary(section => section.Id);

        _columnBlocks = columnBlocks
            .Where(block => !block.IsDeleted && _tables.ContainsKey(block.TableId))
            .ToDictionary(block => block.Id);
    }

    /// <summary>The tables on the sheet, in display order.</summary>
    public IEnumerable<PublishedTableRecord> Tables => _tables.Values.OrderBy(table => table.DisplayOrder).ThenBy(table => table.Id);

    /// <summary>The sections on the sheet, in no particular order.</summary>
    public IEnumerable<PublishedSectionRecord> Sections => _sections.Values;

    /// <summary>The column blocks on the sheet, in no particular order.</summary>
    public IEnumerable<PublishedColumnBlockRecord> ColumnBlocks => _columnBlocks.Values;

    public PublishedColumnBlockRecord? ColumnBlock(int id)
    {
        return _columnBlocks.GetValueOrDefault(id);
    }

    /// <summary>Whether the cell's section, and its column block if it has one, were on the sheet.</summary>
    public bool Shows(PublishedCellRecord cell)
    {
        return _sections.ContainsKey(cell.SectionId)
            && (cell.ColumnBlockId is not { } blockId || _columnBlocks.ContainsKey(blockId));
    }

    /// <summary>Turns the caller's selection into the ids the cell query filters on.</summary>
    public PublishedSelectionScope ScopeFor(PublishedSheetSelection selection)
    {
        var tableIds = _tables.Values
            .Where(table => selection.Tables.Contains(table.PublicId))
            .Select(table => table.Id)
            .ToList();

        var sectionIds = new HashSet<int>();
        var childrenByParent = _sections.Values
            .Where(section => section.ParentSectionId is not null)
            .ToLookup(section => section.ParentSectionId!.Value);
        var pending = new Stack<PublishedSectionRecord>(
            _sections.Values.Where(section => selection.Sections.Contains(section.PublicId)));
        while (pending.Count > 0)
        {
            var section = pending.Pop();
            if (!sectionIds.Add(section.Id))
            {
                continue;
            }

            foreach (var child in childrenByParent[section.Id])
            {
                pending.Push(child);
            }
        }

        return new PublishedSelectionScope(tableIds, [.. sectionIds], [.. selection.Rows], [.. selection.Cells]);
    }

    private bool IsOnSheet(PublishedSectionRecord section, Dictionary<int, PublishedSectionRecord> candidates)
    {
        if (!_tables.ContainsKey(section.TableId))
        {
            return false;
        }

        var current = section;
        var steps = 0;
        while (current.ParentSectionId is { } parentId)
        {
            // A section tree is never deep; the count only guards against a loop in bad data.
            if (!candidates.TryGetValue(parentId, out var parent) || ++steps > candidates.Count)
            {
                return false;
            }

            current = parent;
        }

        return true;
    }
}
