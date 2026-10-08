using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Domain.CellTypes;
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
        Assert.Equal([(2, 15)], collector.Build().Select(source => (source.SheetTableId, source.TemplateCellId)));
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
        var dropdown = new CellType { Kind = CellKind.TextDropdown, Options = [new CellTypeOption { Id = 7, Value = "Pass" }] };

        Assert.Equal("P-1001", LinkedOptionText.Of(new CellValueBag { Text = " P-1001 " }, Type(CellKind.Text)));
        Assert.Equal("P-1001", LinkedOptionText.Of(new CellValueBag { Text = "P-1001" }, Type(CellKind.LinkedDropdown)));
        Assert.Equal("12.5", LinkedOptionText.Of(new CellValueBag { Number = 12.5000000000m }, Type(CellKind.Number)));
        Assert.Equal("2026-10-08", LinkedOptionText.Of(new CellValueBag { Date = new DateOnly(2026, 10, 8) }, Type(CellKind.Date)));
        Assert.Equal("Pass", LinkedOptionText.Of(new CellValueBag { OptionId = 7 }, dropdown));
    }

    [Fact]
    public void AnEmptyCell_OrOneThatCannotBeLinkedTo_IsNoChoice()
    {
        Assert.Null(LinkedOptionText.Of(null, Type(CellKind.Text)));
        Assert.Null(LinkedOptionText.Of(new CellValueBag { Text = "  " }, Type(CellKind.Text)));
        Assert.Null(LinkedOptionText.Of(new CellValueBag { Boolean = true }, Type(CellKind.Checkbox)));
    }

    private static CellType Type(CellKind kind)
    {
        return new CellType { Kind = kind };
    }
}
