using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>Settings chosen on a sheet are checked against the cell's kind and what is on that sheet.</summary>
public sealed class CellInstanceSettingsValidatorTests
{
    private static readonly LinkedDropdownInstanceSettings PartNumbers = new() { SourceSheetTableId = 2, SourceTemplateCellId = 15 };

    private static readonly SheetDto Sheet = SheetWith(
        new SheetLinkedSourceDto(2, 15, ["P-1001", "P-1002"]));

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
    [InlineData(2, null, "both a table and a column")]
    [InlineData(null, 15, "both a table and a column")]
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
        };
        return new SheetDto(1, Guid.Empty, 1, 1, true, null, null, null, 0, [], [table], [])
        {
            LinkedSources = sources,
        };
    }
}
