using PUSpecSheet.Application.Users;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Builds sheet content from a template: a section copy with its rows and cells, then the copies of its
/// sub-sections the template starts with, and so on down. Everything is created as a first draft of the
/// current user, so it stays invisible to others until they publish.
/// </summary>
public sealed class SheetInstantiator(ICurrentUser currentUser)
{
    /// <summary>How many copies of a section a new table or parent starts with. The header is always at least one.</summary>
    public static int StartingCopies(TemplateSection template)
    {
        var minimum = template.Role == SectionRole.Header ? 1 : 0;
        return Math.Max(minimum, template.InitialInstances);
    }

    /// <summary>
    /// Adds the sections a new table starts with (the header, plus any addable sections that start with
    /// copies) to <paramref name="table"/>.
    /// </summary>
    public void AddStartingSections(SheetTable table, TemplateTree tree)
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

    /// <summary>A new copy of a template section, with its rows and its starting sub-sections.</summary>
    public SheetSection NewSection(SheetTable table, TemplateSection template, TemplateTree tree, int displayOrder)
    {
        var section = new SheetSection { SheetTable = table, TemplateSectionId = template.Id };
        section.Revisions.Add(new SheetSectionRevision
        {
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
            var row = NewRow(templateRow, rowOrder);
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

    /// <summary>A new row built from a template row, with one cell per template cell. The caller attaches it to its section.</summary>
    public SheetRow NewRow(TemplateRow templateRow, int displayOrder)
    {
        var row = new SheetRow { TemplateRowId = templateRow.Id };
        foreach (var templateCell in templateRow.Cells.OrderBy(cell => cell.Column).ThenBy(cell => cell.Id))
        {
            row.Cells.Add(new SheetCell { TemplateCellId = templateCell.Id });
        }

        row.Revisions.Add(new SheetRowRevision
        {
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
