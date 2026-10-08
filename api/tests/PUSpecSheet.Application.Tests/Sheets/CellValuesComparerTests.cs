using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>A cell only counts as changed when it differs in a way a person would call a change.</summary>
public sealed class CellValuesComparerTests
{
    private static Dictionary<int, CellValueBag> Values(params (int Cell, CellValueBag Value)[] values)
    {
        return values.ToDictionary(entry => entry.Cell, entry => entry.Value);
    }

    [Fact]
    public void IdenticalValues_AreTheSame()
    {
        var left = Values((1, new CellValueBag { Text = "Bore" }), (2, new CellValueBag { Number = 82.01m }));
        var right = Values((1, new CellValueBag { Text = "Bore" }), (2, new CellValueBag { Number = 82.01m }));

        Assert.True(CellValuesComparer.Same(left, right));
    }

    [Fact]
    public void ADifferentNumber_IsAChange()
    {
        var left = Values((1, new CellValueBag { Number = 82.01m }));
        var right = Values((1, new CellValueBag { Number = 82.02m }));

        Assert.False(CellValuesComparer.Same(left, right));
    }

    [Theory]
    [InlineData("Bore", " Bore ")]
    [InlineData("Bore", "Bore\t")]
    [InlineData("", null)]
    [InlineData("   ", null)]
    public void TextThatDiffersOnlyBySpacesOrEmptiness_IsTheSame(string? published, string? draft)
    {
        var left = Values((1, new CellValueBag { Text = published }));
        var right = Values((1, new CellValueBag { Text = draft }));

        Assert.True(CellValuesComparer.Same(left, right));
    }

    [Fact]
    public void ACellWithNoValue_AndAnEmptyText_AreTheSame()
    {
        Assert.True(CellValuesComparer.Same(Values(), Values((1, new CellValueBag { Text = string.Empty }))));
    }

    [Fact]
    public void AnUntickedBox_IsTheSameAsNothing_ButATickedOneIsNot()
    {
        var published = Values();
        Assert.True(CellValuesComparer.Same(published, Values((1, new CellValueBag { Boolean = false }))));
        Assert.False(CellValuesComparer.Same(published, Values((1, new CellValueBag { Boolean = true }))));
    }

    [Fact]
    public void NumbersWrittenDifferently_AreTheSame()
    {
        var left = Values((1, new CellValueBag { Number = 6.5m }));
        var right = Values((1, new CellValueBag { Number = 6.50m }));

        Assert.True(CellValuesComparer.Same(left, right));
    }

    [Fact]
    public void ADifferentDateOrOption_IsAChange()
    {
        Assert.False(CellValuesComparer.Same(
            Values((1, new CellValueBag { Date = new DateOnly(2026, 10, 1) })),
            Values((1, new CellValueBag { Date = new DateOnly(2026, 10, 2) }))));
        Assert.False(CellValuesComparer.Same(
            Values((1, new CellValueBag { OptionId = 3 })),
            Values((1, new CellValueBag { OptionId = 4 }))));
    }

    [Fact]
    public void ClearingAValue_IsAChange()
    {
        Assert.False(CellValuesComparer.Same(Values((1, new CellValueBag { Number = 5m })), Values()));
    }

    [Fact]
    public void NullDictionaries_AreTreatedAsEmpty()
    {
        Assert.True(CellValuesComparer.Same(null, Values()));
        Assert.True(CellValuesComparer.Same(null, null));
    }

    [Fact]
    public void ADifferentSettingChosenOnTheSheet_IsAChange()
    {
        var partNumbers = new LinkedDropdownInstanceSettings { SourceSheetTableId = 2, SourceTemplateCellId = 15 };
        var left = Values((1, new CellValueBag { Text = "P-1001", Settings = partNumbers }));
        var same = Values((1, new CellValueBag { Text = "P-1001", Settings = partNumbers with { } }));
        var elsewhere = Values((1, new CellValueBag { Text = "P-1001", Settings = partNumbers with { SourceTemplateCellId = 16 } }));
        var cleared = Values((1, new CellValueBag { Text = "P-1001" }));

        Assert.True(CellValuesComparer.Same(left, same));
        Assert.False(CellValuesComparer.Same(left, elsewhere));
        Assert.False(CellValuesComparer.Same(left, cleared));
    }
}
