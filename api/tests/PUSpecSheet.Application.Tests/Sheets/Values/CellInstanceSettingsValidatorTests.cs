using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Tests.Sheets.Values;

/// <summary>Settings chosen on a sheet are checked against the cell's kind and what is on that sheet.</summary>
public sealed class CellInstanceSettingsValidatorTests
{
    private static readonly LinkedDropdownInstanceSettings PartNumbers = new() { SourceSheetTableId = 2, SourceTemplateCellId = 15 };

    private static readonly SheetDto Sheet = SheetWith(
        new SheetLinkedSourceDto(2, 15, null, ["P-1001", "P-1002"]),
        new SheetLinkedSourceDto(2, null, 70, ["0.8", "0.85"]));

    private static readonly LinkedDropdownInstanceSettings PinOffsetRow = new() { SourceSheetTableId = 2, SourceSheetRowId = 70 };

    [Fact]
    public void AColumnOfATableOnTheSheet_CanBeLinkedTo()
    {
        CellInstanceSettingsValidator.Validate("Part", CellKind.LinkedDropdown, PartNumbers, Sheet);
    }

    [Fact]
    public void ClearingSettings_IsAlwaysAllowed()
    {
        CellInstanceSettingsValidator.Validate("Part", CellKind.LinkedDropdown, null, Sheet);
        CellInstanceSettingsValidator.Validate("Part", CellKind.LinkedDropdown, new LinkedDropdownInstanceSettings(), Sheet);
    }

    [Fact]
    public void ACellKindWithNoSettings_RefusesThem()
    {
        var problem = Assert.Throws<InvalidRequestException>(
            () => CellInstanceSettingsValidator.Validate("Description", CellKind.Text, PartNumbers, Sheet));

        Assert.Contains("no settings to choose", problem.Message);
    }

    [Theory]
    [InlineData(2, null, "a column or a row of it")]
    [InlineData(null, 15, "a column or a row of it")]
    [InlineData(9, 15, "isn't on this sheet")]
    [InlineData(2, 99, "isn't on this sheet")]
    public void ALinkedDropdown_NeedsATableAndColumnThatAreOnTheSheet(int? tableId, int? templateCellId, string expected)
    {
        var settings = new LinkedDropdownInstanceSettings { SourceSheetTableId = tableId, SourceTemplateCellId = templateCellId };

        var problem = Assert.Throws<InvalidRequestException>(
            () => CellInstanceSettingsValidator.Validate("Part", CellKind.LinkedDropdown, settings, Sheet));

        Assert.Contains(expected, problem.Message);
    }

    [Fact]
    public void ARowOfATableOnTheSheet_CanBeLinkedTo_AndGivesThatRowsValues()
    {
        CellInstanceSettingsValidator.Validate("Limit", CellKind.LinkedDropdown, PinOffsetRow, Sheet);

        Assert.Equal(["0.8", "0.85"], SheetLinks.OptionsOf(Sheet, PinOffsetRow));
    }

    [Fact]
    public void ARowThatIsNotOnTheSheet_OrNotInThatTable_IsRefused()
    {
        var problem = Assert.Throws<InvalidRequestException>(
            () => CellInstanceSettingsValidator.Validate("Limit", CellKind.LinkedDropdown, PinOffsetRow with { SourceSheetRowId = 71 }, Sheet));

        Assert.Contains("isn't on this sheet", problem.Message);
        Assert.Null(SheetLinks.OptionsOf(Sheet, PinOffsetRow with { SourceSheetTableId = 9 }));
    }

    [Fact]
    public void AColumnAndARowTogether_AreRefused()
    {
        var problem = Assert.Throws<InvalidRequestException>(
            () => CellInstanceSettingsValidator.Validate("Limit", CellKind.LinkedDropdown, PinOffsetRow with { SourceTemplateCellId = 15 }, Sheet));

        Assert.Contains("a column or from a row, not both", problem.Message);
    }

    [Fact]
    public void TheChoicesOfSettings_AreTheirColumnsValues()
    {
        Assert.Equal(["P-1001", "P-1002"], SheetLinks.OptionsOf(Sheet, PartNumbers));
    }

    [Fact]
    public void AColumnNothingHasBeenEnteredIn_HasNoChoices_ButIsNotBroken()
    {
        Assert.Empty(SheetLinks.OptionsOf(SheetWith(), PartNumbers)!);
    }

    [Fact]
    public void SettingsWhoseTableHasGone_HaveNoChoicesAtAll()
    {
        var removed = PartNumbers with { SourceSheetTableId = 9 };

        Assert.Null(SheetLinks.OptionsOf(Sheet, removed));
        Assert.Null(SheetLinks.OptionsOf(Sheet, null));
    }

    private static SheetDto SheetWith(params SheetLinkedSourceDto[] sources)
    {
        var table = new SheetTableDto(2, Guid.Empty, 1, "Parts limits", 1, TemplateOrientation.Vertical, 0, null, 1, null, false, [], [], [], [])
        {
            LinkableColumns = [new SheetLinkableColumnDto(15, "Part number")],
            LinkableRows = [new SheetLinkableRowDto(70, "Pin offset (mm)")],
        };
        return new SheetDto(1, Guid.Empty, 1, 1, true, null, null, null, 0, [], [table], [])
        {
            LinkedSources = sources,
        };
    }
}
