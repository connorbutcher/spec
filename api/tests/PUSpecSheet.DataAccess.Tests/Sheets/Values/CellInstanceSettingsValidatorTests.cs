using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Linking;
using PUSpecSheet.DataAccess.Sheets.Values;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Values;

/// <summary>Settings chosen on a sheet are checked against the cell's kind and what is on that sheet.</summary>
public sealed class CellInstanceSettingsValidatorTests
{
    private static readonly LinkedDropdownInstanceSettings _partNumbers = new() { SourceSheetTableId = 2, SourceTemplateCellId = 15 };

    private static readonly SheetDto _sheet = SheetWith(
        new SheetLinkedSourceDto { SheetTableId = 2, TemplateCellId = 15, Options = ["P-1001", "P-1002"] },
        new SheetLinkedSourceDto { SheetTableId = 2, SheetRowId = 70, Options = ["0.8", "0.85"] });

    private static readonly LinkedDropdownInstanceSettings _pinOffsetRow = new() { SourceSheetTableId = 2, SourceSheetRowId = 70 };

    [Fact]
    public void AColumnOfATableOnTheSheet_CanBeLinkedTo()
    {
        CellInstanceSettingsValidator.Validate("Part", CellKind.LinkedDropdown, _partNumbers, _sheet);
    }

    [Fact]
    public void ClearingSettings_IsAlwaysAllowed()
    {
        CellInstanceSettingsValidator.Validate("Part", CellKind.LinkedDropdown, null, _sheet);
        CellInstanceSettingsValidator.Validate("Part", CellKind.LinkedDropdown, new LinkedDropdownInstanceSettings(), _sheet);
    }

    [Fact]
    public void ACellKindWithNoSettings_RefusesThem()
    {
        var problem = Assert.Throws<InvalidRequestException>(
            () => CellInstanceSettingsValidator.Validate("Description", CellKind.Text, _partNumbers, _sheet));

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
            () => CellInstanceSettingsValidator.Validate("Part", CellKind.LinkedDropdown, settings, _sheet));

        Assert.Contains(expected, problem.Message);
    }

    [Fact]
    public void ARowOfATableOnTheSheet_CanBeLinkedTo_AndGivesThatRowsValues()
    {
        CellInstanceSettingsValidator.Validate("Limit", CellKind.LinkedDropdown, _pinOffsetRow, _sheet);

        Assert.Equal(["0.8", "0.85"], SheetLinks.OptionsOf(_sheet, _pinOffsetRow));
    }

    [Fact]
    public void ARowThatIsNotOnTheSheet_OrNotInThatTable_IsRefused()
    {
        var problem = Assert.Throws<InvalidRequestException>(
            () => CellInstanceSettingsValidator.Validate("Limit", CellKind.LinkedDropdown, _pinOffsetRow with { SourceSheetRowId = 71 }, _sheet));

        Assert.Contains("isn't on this sheet", problem.Message);
        Assert.Null(SheetLinks.OptionsOf(_sheet, _pinOffsetRow with { SourceSheetTableId = 9 }));
    }

    [Fact]
    public void AColumnAndARowTogether_AreRefused()
    {
        var problem = Assert.Throws<InvalidRequestException>(
            () => CellInstanceSettingsValidator.Validate("Limit", CellKind.LinkedDropdown, _pinOffsetRow with { SourceTemplateCellId = 15 }, _sheet));

        Assert.Contains("a column or from a row, not both", problem.Message);
    }

    [Fact]
    public void TheChoicesOfSettings_AreTheirColumnsValues()
    {
        Assert.Equal(["P-1001", "P-1002"], SheetLinks.OptionsOf(_sheet, _partNumbers));
    }

    [Fact]
    public void AColumnNothingHasBeenEnteredIn_HasNoChoices_ButIsNotBroken()
    {
        Assert.Empty(SheetLinks.OptionsOf(SheetWith(), _partNumbers)!);
    }

    [Fact]
    public void SettingsWhoseTableHasGone_HaveNoChoicesAtAll()
    {
        var removed = _partNumbers with { SourceSheetTableId = 9 };

        Assert.Null(SheetLinks.OptionsOf(_sheet, removed));
        Assert.Null(SheetLinks.OptionsOf(_sheet, null));
    }

    private static SheetDto SheetWith(params SheetLinkedSourceDto[] sources)
    {
        var table = new SheetTableDto
        {
            Id = 2,
            PublicId = Guid.Empty,
            TableTemplateId = 1,
            TemplateName = "Parts limits",
            TemplateVersionNumber = 1,
            Orientation = TemplateOrientation.Vertical,
            StickyColumnCount = 0,
            DisplayOrder = 1,
            IsPending = false,
            Sections = [],
            AddableSections = [],
            ColumnBlocks = [],
            AddableColumnBlocks = [],
            LinkableColumns = [new SheetLinkableColumnDto { TemplateCellId = 15, Label = "Part number" }],
            LinkableRows = [new SheetLinkableRowDto { SheetRowId = 70, Label = "Pin offset (mm)" }],
        };
        return new SheetDto
        {
            Id = 1,
            PublicId = Guid.Empty,
            PhaseId = 1,
            SheetTypeId = 1,
            IsLive = true,
            MyDraftCount = 0,
            Versions = [],
            Tables = [table],
            AvailableTemplates = [],
            LinkedSources = sources,
        };
    }
}
