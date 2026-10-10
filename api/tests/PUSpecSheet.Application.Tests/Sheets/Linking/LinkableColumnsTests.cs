using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Tests.Sheets.Linking;

/// <summary>Which columns of a table a linked dropdown can read, and what each is called.</summary>
public sealed class LinkableColumnsTests
{
    private const int Heading = 1;
    private const int Text = 2;
    private const int Number = 3;
    private const int Checkbox = 4;

    private static readonly Dictionary<int, CellType> CellTypes = new()
    {
        [Heading] = new CellType { Id = Heading, Name = "Heading", Kind = CellKind.Heading },
        [Text] = new CellType { Id = Text, Name = "Text", Kind = CellKind.Text },
        [Number] = new CellType { Id = Number, Name = "Number", Kind = CellKind.Number },
        [Checkbox] = new CellType { Id = Checkbox, Name = "Checkbox", Kind = CellKind.Checkbox },
    };

    private static readonly TemplateSection Header = new() { Id = 1, Name = "Header", Role = SectionRole.Header, DisplayOrder = 1 };
    private static readonly TemplateSection Parts = new() { Id = 2, Name = "Parts", Role = SectionRole.Addable, DisplayOrder = 2 };
    private static readonly TemplateSection Spares = new() { Id = 3, Name = "Spares", Role = SectionRole.Addable, DisplayOrder = 3 };

    [Fact]
    public void AColumn_IsNamedByItsCaptionThenItsLookupKeyThenTheHeadingAboveItThenItsCellType()
    {
        var rows = new[]
        {
            Row(1, Header, Cell(10, Heading, 1, caption: "Part number"), Cell(11, Heading, 2, caption: "Notes", columnSpan: 2)),
            Row(2, Parts,
                Cell(20, Text, 1, caption: "Number of the part", lookupKey: "partNumber"),
                Cell(21, Text, 2, lookupKey: "note"),
                Cell(22, Text, 3),
                Cell(23, Number, 4)),
        };

        var columns = LinkableColumns.For([Header, Parts], rows, CellTypes);

        Assert.Equal(
            [(20, "Number of the part"), (21, "note"), (22, "Notes"), (23, "Number")],
            columns.Select(column => (column.TemplateCellId, column.Label)));
    }

    [Fact]
    public void HeadingsGroupsAndCheckboxes_AreNotColumnsToLinkTo()
    {
        var rows = new[]
        {
            Row(1, Header, Cell(10, Heading, 1, caption: "Fitted")),
            Row(2, Parts, Cell(20, Checkbox, 1), Cell(21, Text, 2)),
        };

        var columns = LinkableColumns.For([Header, Parts], rows, CellTypes);

        Assert.Equal([21], columns.Select(column => column.TemplateCellId));
    }

    [Fact]
    public void ColumnsThatWouldReadTheSame_AreToldApartByTheirSection()
    {
        var rows = new[]
        {
            Row(1, Header, Cell(10, Heading, 1, caption: "Part number")),
            Row(2, Parts, Cell(20, Text, 1)),
            Row(3, Spares, Cell(30, Text, 1)),
        };

        var columns = LinkableColumns.For([Header, Parts, Spares], rows, CellTypes);

        Assert.Equal(["Part number (Parts)", "Part number (Spares)"], columns.Select(column => column.Label));
    }

    [Fact]
    public void AHeading_NamesOnlyTheColumnsOfItsOwnColumnBlock()
    {
        var rows = new[]
        {
            Row(1, Header, Cell(10, Heading, 1, caption: "Description"), Cell(11, Heading, 1, caption: "Min", blockId: 7)),
            Row(2, Parts, Cell(20, Text, 1), Cell(21, Number, 1, blockId: 7)),
        };

        var columns = LinkableColumns.For([Header, Parts], rows, CellTypes);

        Assert.Equal(["Description", "Min"], columns.Select(column => column.Label));
    }

    [Fact]
    public void RowsOfAnotherTemplateVersion_AreLeftOut()
    {
        var elsewhere = new TemplateSection { Id = 99, Name = "Elsewhere", Role = SectionRole.Addable };
        var rows = new[] { Row(1, Parts, Cell(20, Text, 1)), Row(2, elsewhere, Cell(30, Text, 1)) };

        var columns = LinkableColumns.For([Parts], rows, CellTypes);

        Assert.Equal([20], columns.Select(column => column.TemplateCellId));
    }

    private static TemplateRow Row(int id, TemplateSection section, params TemplateCell[] cells)
    {
        return new TemplateRow { Id = id, TemplateSectionId = section.Id, DisplayOrder = id, Cells = cells };
    }

    private static TemplateCell Cell(int id, int cellTypeId, int column, string? caption = null, string? lookupKey = null, int columnSpan = 1, int? blockId = null)
    {
        return new TemplateCell
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
