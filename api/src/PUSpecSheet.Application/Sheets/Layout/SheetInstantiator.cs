using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Layout;

/// <summary>
/// Builds sheet content from a template: a section copy with its rows and cells, then the copies of its
/// sub-sections the template starts with, and so on down. Everything is created as a first draft of the
/// current user, so it stays invisible to others until they publish.
/// </summary>
public sealed class SheetInstantiator
{
    private readonly ICurrentUser currentUser;

    public SheetInstantiator(ICurrentUser currentUser)
    {
        this.currentUser = currentUser;
    }

    /// <summary>How many copies of a section a new table or parent starts with. The header is always at least one.</summary>
    public static int StartingCopies(DbTemplateSection template)
    {
        var minimum = template.Role == SectionRole.Header ? 1 : 0;
        return Math.Max(minimum, template.InitialInstances);
    }

    /// <summary>
    /// Adds the sections a new table starts with (the header, plus any addable sections that start with
    /// copies) to <paramref name="table"/>.
    /// </summary>
    public void AddStartingSections(DbSheetTable table, TemplateTree tree)
    {
        var order = 0;
        foreach (var template in tree.ChildrenOf(null))
        {
            for (var copy = 0; copy < StartingCopies(template); copy++)
            {
                order += OrderGaps.Spacing;
                table.Sections.Add(NewSection(table, template, tree, order));
            }
        }
    }

    /// <summary>How many copies of a column block a new horizontal table starts with.</summary>
    public static int StartingCopies(DbTemplateColumnBlock template)
    {
        return Math.Max(template.MinInstances, template.InitialInstances);
    }

    /// <summary>Adds the column block copies a new horizontal table starts with to <paramref name="table"/>.</summary>
    public void AddStartingColumnBlocks(DbSheetTable table, TemplateTree tree)
    {
        var order = 0;
        foreach (var template in tree.ColumnBlocks)
        {
            for (var copy = 0; copy < StartingCopies(template); copy++)
            {
                order += OrderGaps.Spacing;
                table.ColumnBlocks.Add(NewColumnBlock(0, table.SheetId, template, order, table));
            }
        }
    }

    /// <summary>A new copy of a template column block, as its author's first draft. Its cells are added by the cell filler.</summary>
    public DbSheetColumnBlock NewColumnBlock(int tableId, int sheetId, DbTemplateColumnBlock template, int displayOrder)
    {
        return NewColumnBlock(tableId, sheetId, template, displayOrder, null);
    }

    /// <summary>A new copy of a template section, with its rows and its starting sub-sections.</summary>
    public DbSheetSection NewSection(DbSheetTable table, DbTemplateSection template, TemplateTree tree, int displayOrder)
    {
        var section = new DbSheetSection { SheetTable = table, TemplateSectionId = template.Id };
        section.Revisions.Add(new DbSheetSectionRevision
        {
            SheetId = table.SheetId,
            RevisionNumber = 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = displayOrder,
            AuthorUserId = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        var rowOrder = 0;
        foreach (var templateRow in template.Rows.OrderBy(row => row.DisplayOrder).ThenBy(row => row.Id))
        {
            rowOrder += OrderGaps.Spacing;
            var row = NewRow(templateRow, table.SheetId, rowOrder);
            row.SheetSection = section;
            section.Rows.Add(row);
        }

        var childOrder = 0;
        foreach (var childTemplate in tree.ChildrenOf(template.Id))
        {
            for (var copy = 0; copy < StartingCopies(childTemplate); copy++)
            {
                childOrder += OrderGaps.Spacing;
                var child = NewSection(table, childTemplate, tree, childOrder);
                child.ParentSheetSection = section;
                section.ChildSheetSections.Add(child);
            }
        }

        return section;
    }

    private DbSheetColumnBlock NewColumnBlock(int tableId, int sheetId, DbTemplateColumnBlock template, int displayOrder, DbSheetTable? table)
    {
        var block = new DbSheetColumnBlock { TemplateColumnBlockId = template.Id };
        if (table is null)
        {
            block.SheetTableId = tableId;
        }
        else
        {
            block.SheetTable = table;
        }

        block.Revisions.Add(new DbSheetColumnBlockRevision
        {
            SheetId = sheetId,
            RevisionNumber = 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = displayOrder,
            AuthorUserId = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });
        return block;
    }

    /// <summary>
    /// A new row built from a template row, with a cell for each of its own template cells. Its cells in
    /// column blocks are added by the cell filler, since they depend on how many copies the table has. The
    /// caller attaches the row to its section.
    /// </summary>
    public DbSheetRow NewRow(DbTemplateRow templateRow, int sheetId, int displayOrder)
    {
        var row = new DbSheetRow { TemplateRowId = templateRow.Id };
        foreach (var templateCell in templateRow.Cells.Where(cell => cell.TemplateColumnBlockId is null).OrderBy(cell => cell.Column).ThenBy(cell => cell.Id))
        {
            row.Cells.Add(new DbSheetCell { TemplateCellId = templateCell.Id });
        }

        row.Revisions.Add(new DbSheetRowRevision
        {
            SheetId = sheetId,
            RevisionNumber = 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = displayOrder,
            AuthorUserId = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });
        return row;
    }
}
