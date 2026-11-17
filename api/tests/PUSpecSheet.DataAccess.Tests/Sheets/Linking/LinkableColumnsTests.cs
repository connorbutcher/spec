using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Sheets.Linking;
using PUSpecSheet.Database.Models.CellTypes;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Linking;

/// <summary>Which columns of a table a linked dropdown can read, and what each is called.</summary>
public sealed class LinkableColumnsTests
{
    private const int Heading = 1;
    private const int Text = 2;
    private const int Number = 3;
    private const int Checkbox = 4;

    private static readonly Dictionary<int, DbCellType> _cellTypes = new()
    {
        [Heading] = new DbCellType { Id = Heading, Name = "Heading", Kind = CellKind.Heading },
        [Text] = new DbCellType { Id = Text, Name = "Text", Kind = CellKind.Text },
        [Number] = new DbCellType { Id = Number, Name = "Number", Kind = CellKind.Number },
        [Checkbox] = new DbCellType { Id = Checkbox, Name = "Checkbox", Kind = CellKind.Checkbox },
    };

    private static readonly DbTemplateSection _header = new() { Id = 1, Name = "Header", Role = SectionRole.Header, DisplayOrder = 1 };
    private static readonly DbTemplateSection _parts = new() { Id = 2, Name = "Parts", Role = SectionRole.Addable, DisplayOrder = 2 };
    private static readonly DbTemplateSection _spares = new() { Id = 3, Name = "Spares", Role = SectionRole.Addable, DisplayOrder = 3 };

    [Fact]
    public void AColumn_IsNamedByItsCaptionThenItsLookupKeyThenTheHeadingAboveItThenItsCellType()
    {
        var rows = new[]
        {
            Row(1, _header, Cell(10, Heading, 1, caption: "Part number"), Cell(11, Heading, 2, caption: "Notes", columnSpan: 2)),
            Row(2, _parts,
                Cell(20, Text, 1, caption: "Number of the part", lookupKey: "partNumber"),
                Cell(21, Text, 2, lookupKey: "note"),
                Cell(22, Text, 3),
                Cell(23, Number, 4)),
        };

        var columns = LinkableColumns.For([_header, _parts], rows, _cellTypes);

        Assert.Equal(
            [(20, "Number of the part"), (21, "note"), (22, "Notes"), (23, "Number")],
            columns.Select(column => (column.TemplateCellId, column.Label)));
    }

    [Fact]
    public void HeadingsGroupsAndCheckboxes_AreNotColumnsToLinkTo()
    {
        var rows = new[]
        {
            Row(1, _header, Cell(10, Heading, 1, caption: "Fitted")),
            Row(2, _parts, Cell(20, Checkbox, 1), Cell(21, Text, 2)),
        };

        var columns = LinkableColumns.For([_header, _parts], rows, _cellTypes);

        Assert.Equal([21], columns.Select(column => column.TemplateCellId));
    }

    [Fact]
    public void ColumnsThatWouldReadTheSame_AreToldApartByTheirSection()
    {
        var rows = new[]
        {
            Row(1, _header, Cell(10, Heading, 1, caption: "Part number")),
            Row(2, _parts, Cell(20, Text, 1)),
            Row(3, _spares, Cell(30, Text, 1)),
        };

        var columns = LinkableColumns.For([_header, _parts, _spares], rows, _cellTypes);

        Assert.Equal(["Part number (Parts)", "Part number (Spares)"], columns.Select(column => column.Label));
    }

    [Fact]
    public void AHeading_NamesOnlyTheColumnsOfItsOwnColumnBlock()
    {
        var rows = new[]
        {
            Row(1, _header, Cell(10, Heading, 1, caption: "Description"), Cell(11, Heading, 1, caption: "Min", blockId: 7)),
            Row(2, _parts, Cell(20, Text, 1), Cell(21, Number, 1, blockId: 7)),
        };

        var columns = LinkableColumns.For([_header, _parts], rows, _cellTypes);

        Assert.Equal(["Description", "Min"], columns.Select(column => column.Label));
    }

    [Fact]
    public void RowsOfAnotherTemplateVersion_AreLeftOut()
    {
        var elsewhere = new DbTemplateSection { Id = 99, Name = "Elsewhere", Role = SectionRole.Addable };
        var rows = new[] { Row(1, _parts, Cell(20, Text, 1)), Row(2, elsewhere, Cell(30, Text, 1)) };

        var columns = LinkableColumns.For([_parts], rows, _cellTypes);

        Assert.Equal([20], columns.Select(column => column.TemplateCellId));
    }

    private static DbTemplateRow Row(int id, DbTemplateSection section, params DbTemplateCell[] cells)
    {
        return new DbTemplateRow { Id = id, TemplateSectionId = section.Id, DisplayOrder = id, Cells = cells };
    }

    private static DbTemplateCell Cell(int id, int cellTypeId, int column, string? caption = null, string? lookupKey = null, int columnSpan = 1, int? blockId = null)
    {
        return new DbTemplateCell
        {
            Id = id,
            CellTypeId = cellTypeId,
            Column = column,
            ColumnSpan = columnSpan,
            Caption = caption,
            LookupKey = lookupKey,
            TemplateColumnBlockId = blockId,
        };
    }
}
