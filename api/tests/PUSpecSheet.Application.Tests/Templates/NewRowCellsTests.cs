using PUSpecSheet.Application.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Tests.Templates;

/// <summary>The cells a new template row starts with.</summary>
public sealed class NewRowCellsTests
{
    private const int DefaultCellType = 2;

    [Fact]
    public void WithNothingToCopy_TheRowGetsOneCellOfItsOwnAndOnePerColumnBlock()
    {
        var cells = NewRowCells.For([], null, 0, DefaultCellType, [10, 11]);

        Assert.Equal([null, 10, 11], cells.Select(cell => cell.TemplateColumnBlockId));
        Assert.All(cells, cell =>
        {
            Assert.Equal(1, cell.Column);
            Assert.Equal(DefaultCellType, cell.CellTypeId);
        });
    }

    [Fact]
    public void CopyingARow_TakesItsLayoutAndTypes_ButNotItsCaptionsOrSettings()
    {
        var source = Row(
            new TemplateCell { Column = 1, ColumnSpan = 2, CellTypeId = 5, Caption = "Bore", IsRequired = true, LookupKey = "bore" },
            new TemplateCell { Column = 1, CellTypeId = 6, TemplateColumnBlockId = 10 });

        var cells = NewRowCells.For([source], source, 1, DefaultCellType, [10]);

        Assert.Equal(2, cells.Count);
        Assert.Equal((1, 2, 5), (cells[0].Column, cells[0].ColumnSpan, cells[0].CellTypeId));
        Assert.Null(cells[0].Caption);
        Assert.False(cells[0].IsRequired);
        Assert.Null(cells[0].LookupKey);
        Assert.Equal(10, cells[1].TemplateColumnBlockId);
    }

    [Fact]
    public void ACellSpanningDownFromARowAbove_LeavesNoCellUnderIt()
    {
        var tall = Row(
            new TemplateCell { Column = 1, RowSpan = 2, CellTypeId = 5 },
            new TemplateCell { Column = 2, CellTypeId = 5 });

        var cells = NewRowCells.For([tall], tall, 1, DefaultCellType, []);

        Assert.Equal([2], cells.Select(cell => cell.Column));
    }

    [Fact]
    public void ASpanThatEndsBeforeTheNewRow_CoversNothing()
    {
        var tall = Row(new TemplateCell { Column = 1, RowSpan = 2, CellTypeId = 5 });
        var below = Row();

        var cells = NewRowCells.For([tall, below], tall, 2, DefaultCellType, []);

        Assert.Equal([1], cells.Select(cell => cell.Column));
    }

    private static TemplateRow Row(params TemplateCell[] cells)
    {
        return new TemplateRow { Cells = cells.ToList() };
    }
}
