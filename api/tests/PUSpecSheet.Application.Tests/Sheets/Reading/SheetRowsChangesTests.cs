using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Tests.Sheets.Reading;

/// <summary>Rows taken out of a sheet carry what follows from them, and nothing of the rows not asked for.</summary>
public sealed class SheetRowsChangesTests
{
    [Fact]
    public void TheRowsAskedFor_AreTakenFromAnyDepth_WithTheirTablesLinkableRows()
    {
        var sheet = Sheet();

        var change = SheetRowsChanges.From(sheet, [21, 99]);

        Assert.Equal([21], change.Rows.Select(row => row.Id));
        Assert.Equal([2], change.LinkableRows.Select(table => table.SheetTableId));
        Assert.Equal([201], change.TemplateCells.Select(templateCell => templateCell.Id));
        Assert.Equal(4, change.MyDraftCount);
        Assert.Equal(7, change.LatestVersionNumber);
        Assert.Same(sheet, change.Sheet);
    }

    [Fact]
    public void ARowNotOnTheSheet_IsLeftOut()
    {
        var change = SheetRowsChanges.From(Sheet(), [99]);

        Assert.Empty(change.Rows);
        Assert.Empty(change.LinkableRows);
    }

    private static SheetDto Sheet()
    {
        var first = Table(1, Section(1, [Row(11, 101)], []));
        var second = Table(2, Section(2, [], [Section(3, [Row(21, 201)], [])]));
        return new SheetDto(5, Guid.Empty, 1, 1, true, null, null, 7, 4, [], [first, second], [])
        {
            TemplateCells = [TemplateCell(101), TemplateCell(201)],
        };
    }

    private static SheetTableDto Table(int id, SheetSectionDto section)
    {
        return new SheetTableDto(id, Guid.Empty, 1, "Limits", 1, TemplateOrientation.Vertical, 0, null, 1, null, false, [section], [], [], [])
        {
            LinkableRows = [new SheetLinkableRowDto(id * 10, "Row")],
        };
    }

    private static SheetSectionDto Section(int id, IReadOnlyList<SheetRowDto> rows, IReadOnlyList<SheetSectionDto> sections)
    {
        return new SheetSectionDto(id, Guid.Empty, 1, "Limits", SectionRole.Addable, 0, null, 1, 1, null, false, true, rows, sections, [], [], null);
    }

    private static SheetRowDto Row(int id, int templateCellId)
    {
        var cell = new SheetCellDto(id * 10, Guid.Empty, templateCellId, "x", null, null);
        return new SheetRowDto(id, Guid.Empty, 1, 1, null, false, CanRemove: true, [cell], null);
    }

    private static TemplateCellDto TemplateCell(int id)
    {
        return new TemplateCellDto(id, 1, 1, 1, 1, null, false, null, null, null);
    }
}
