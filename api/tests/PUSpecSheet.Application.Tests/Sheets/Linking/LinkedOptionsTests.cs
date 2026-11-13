using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Database.Models.CellTypes;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Tests.Sheets.Linking;

/// <summary>The choices a linked dropdown offers are the values of the column it is pointed at.</summary>
public sealed class LinkedOptionsTests
{
    private static readonly CellValueBag LinkedToPartNumbers = new()
    {
        Settings = new LinkedDropdownInstanceSettings { SourceSheetTableId = 2, SourceTemplateCellId = 15 },
    };

    [Fact]
    public void OnlyColumnsSomethingIsLinkedTo_AreGathered()
    {
        var collector = new LinkedOptionCollector([LinkedToPartNumbers, new CellValueBag { Text = "no settings" }]);

        Assert.True(collector.Wants(2, 15));
        Assert.False(collector.Wants(2, 16));
        Assert.False(collector.Wants(3, 15));

        collector.Add(2, 16, "ignored");
        Assert.Equal([(2, (int?)15)], collector.Build().Select(source => (source.SheetTableId, source.TemplateCellId)));
    }

    [Fact]
    public void Choices_KeepTheOrderTheyAreMetIn_WithRepeatsLeftOut()
    {
        var collector = new LinkedOptionCollector([LinkedToPartNumbers]);

        collector.Add(2, 15, "P-1002");
        collector.Add(2, 15, "P-1001");
        collector.Add(2, 15, "P-1002");

        Assert.Equal(["P-1002", "P-1001"], collector.Build().Single().Options);
    }

    [Fact]
    public void ARowSomethingIsLinkedTo_GivesItsValues_WithoutRepeats()
    {
        var linkedToRow = new CellValueBag
        {
            Settings = new LinkedDropdownInstanceSettings { SourceSheetTableId = 2, SourceSheetRowId = 70 },
        };
        var collector = new LinkedOptionCollector([LinkedToPartNumbers, linkedToRow]);

        Assert.True(collector.WantsRow(2, 70));
        Assert.False(collector.WantsRow(2, 71));
        Assert.False(collector.WantsRow(3, 70));

        collector.AddToRow(2, 70, "0.8");
        collector.AddToRow(2, 70, "0.85");
        collector.AddToRow(2, 70, "0.8");
        collector.AddToRow(2, 71, "ignored");

        var row = collector.Build().Single(source => source.SheetRowId == 70);
        Assert.Null(row.TemplateCellId);
        Assert.Equal(["0.8", "0.85"], row.Options);
    }

    [Fact]
    public void TheFirstValueOfARow_NamesIt_AndTheRestAreItsChoices()
    {
        string[] values = ["Pin offset (mm)", "0.8", "0.85"];

        Assert.Equal("Pin offset (mm)", LinkedRowValues.NameOf(values));
        Assert.Equal(["0.8", "0.85"], LinkedRowValues.ChoicesOf(values));
        Assert.Null(LinkedRowValues.NameOf([]));
    }

    [Fact]
    public void AColumnWithNoValuesYet_IsStillListed_WithNoChoices()
    {
        var collector = new LinkedOptionCollector([LinkedToPartNumbers]);

        Assert.Empty(collector.Build().Single().Options);
    }

    [Fact]
    public void SettingsThatPointNowhere_AskForNothing()
    {
        var halfChosen = new CellValueBag { Settings = new LinkedDropdownInstanceSettings { SourceSheetTableId = 2 } };

        Assert.Empty(new LinkedOptionCollector([halfChosen]).Build());
    }

    [Fact]
    public void AValue_ReadsAsAChoiceTheWayItsKindIsWritten()
    {
        var dropdown = new DbCellType { Kind = CellKind.TextDropdown, Options = [new DbCellTypeOption { Id = 7, Value = "Pass" }] };

        Assert.Equal("P-1001", LinkedOptionText.Of(new CellValueBag { Text = " P-1001 " }, Type(CellKind.Text)));
        Assert.Equal("P-1001", LinkedOptionText.Of(new CellValueBag { Text = "P-1001" }, Type(CellKind.LinkedDropdown)));
        Assert.Equal("12.5", LinkedOptionText.Of(new CellValueBag { Number = 12.5000000000m }, new DbCellType { Kind = CellKind.Number, Configuration = new NumberCellConfiguration() }));
        Assert.Equal("2026-10-08", LinkedOptionText.Of(new CellValueBag { Date = new DateOnly(2026, 10, 8) }, Type(CellKind.Date)));
        Assert.Equal("Pass", LinkedOptionText.Of(new CellValueBag { OptionId = 7 }, dropdown));
    }

    [Fact]
    public void ANumber_ReadsToItsCellsDecimalPlaces_AsItDoesOnTheSheet()
    {
        var twoPlaces = new DbCellType { Kind = CellKind.Number, Configuration = new NumberCellConfiguration { DecimalPlaces = 2 } };

        Assert.Equal("0.80", LinkedOptionText.Of(new CellValueBag { Number = 0.8m }, twoPlaces));
        Assert.Equal("82.00", LinkedOptionText.Of(new CellValueBag { Number = 82m }, twoPlaces));
        Assert.Equal("6", LinkedOptionText.Of(new CellValueBag { Number = 6m }, twoPlaces, new NumberCellConfiguration { DecimalPlaces = 0 }));
    }

    [Fact]
    public void AnEmptyCell_OrOneThatCannotBeLinkedTo_IsNoChoice()
    {
        Assert.Null(LinkedOptionText.Of(null, Type(CellKind.Text)));
        Assert.Null(LinkedOptionText.Of(new CellValueBag { Text = "  " }, Type(CellKind.Text)));
        Assert.Null(LinkedOptionText.Of(new CellValueBag { Boolean = true }, Type(CellKind.Checkbox)));
    }

    private static DbCellType Type(CellKind kind)
    {
        return new DbCellType { Kind = kind };
    }
}
