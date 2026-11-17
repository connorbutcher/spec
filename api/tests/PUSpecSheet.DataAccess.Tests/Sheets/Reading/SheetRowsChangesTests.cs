using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Sheets.Reading;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Reading;

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

    private static Sheet Sheet()
    {
        var first = Table(1, Section(1, [Row(11, 101)], []));
        var second = Table(2, Section(2, [], [Section(3, [Row(21, 201)], [])]));
        return new Sheet
        {
            Id = 5,
            PublicId = Guid.Empty,
            PhaseId = 1,
            SheetTypeId = 1,
            IsLive = true,
            LatestVersionNumber = 7,
            MyDraftCount = 4,
            Versions = [],
            Tables = [first, second],
            AvailableTemplates = [],
            TemplateCells = [TemplateCell(101), TemplateCell(201)],
        };
    }

    private static SheetTable Table(int id, SheetSection section)
    {
        return new SheetTable
        {
            Id = id,
            PublicId = Guid.Empty,
            TableTemplateId = 1,
            TemplateName = "Limits",
            TemplateVersionNumber = 1,
            Orientation = TemplateOrientation.Vertical,
            StickyColumnCount = 0,
            DisplayOrder = 1,
            IsPending = false,
            Sections = [section],
            AddableSections = [],
            ColumnBlocks = [],
            AddableColumnBlocks = [],
            LinkableRows = [new SheetLinkableRow { SheetRowId = id * 10, Label = "Row" }],
        };
    }

    private static SheetSection Section(int id, IReadOnlyList<SheetRow> rows, IReadOnlyList<SheetSection> sections)
    {
        return new SheetSection
        {
            Id = id,
            PublicId = Guid.Empty,
            TemplateSectionId = 1,
            Name = "Limits",
            Role = SectionRole.Addable,
            MinInstances = 0,
            InitialInstances = 1,
            DisplayOrder = 1,
            IsPending = false,
            CanRemove = true,
            Rows = rows,
            Sections = sections,
            AddableSections = [],
        };
    }

    private static SheetRow Row(int id, int templateCellId)
    {
        var cell = new SheetCell
        {
            Id = id * 10,
            PublicId = Guid.Empty,
            TemplateCellId = templateCellId,
            Value = "x",
        };
        return new SheetRow
        {
            Id = id,
            PublicId = Guid.Empty,
            TemplateRowId = 1,
            DisplayOrder = 1,
            IsPending = false,
            CanRemove = true,
            Cells = [cell],
        };
    }

    private static TemplateCell TemplateCell(int id)
    {
        return new TemplateCell
        {
            Id = id,
            CellTypeId = 1,
            Column = 1,
            RowSpan = 1,
            ColumnSpan = 1,
            IsRequired = false,
        };
    }
}
