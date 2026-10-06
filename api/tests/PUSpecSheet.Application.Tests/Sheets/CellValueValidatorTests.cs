using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Tests.Sheets;

/// <summary>A value typed into a sheet cell is checked against the cell's kind and its settings.</summary>
public sealed class CellValueValidatorTests
{
    [Theory]
    [InlineData(CellKind.Heading)]
    [InlineData(CellKind.Group)]
    public void ACellThatHoldsNoValue_RefusesOne(CellKind kind)
    {
        var cell = Cell(kind, CellConfigurations.CreateEmpty(kind));

        Assert.Throws<InvalidRequestException>(() => CellValueValidator.Validate(cell, Value(text: "x")));
    }

    [Fact]
    public void Text_IsHeldToTheCellTypesMaxLength()
    {
        var cell = Cell(CellKind.Text, new TextCellConfiguration { MaxLength = 5 });

        CellValueValidator.Validate(cell, Value(text: "12345"));
        var problem = Assert.Throws<InvalidRequestException>(() => CellValueValidator.Validate(cell, Value(text: "123456")));

        Assert.Contains("at most 5 characters", problem.Message);
    }

    [Fact]
    public void ACellsOwnSetting_ReplacesItsTypes()
    {
        var cell = Cell(CellKind.Text, new TextCellConfiguration { MaxLength = 5 });
        cell.ConfigurationOverride = new TextCellConfiguration { MaxLength = 10 };

        CellValueValidator.Validate(cell, Value(text: "1234567890"));
    }

    [Theory]
    [InlineData(9.99, "can't be less than 10")]
    [InlineData(20.01, "can't be more than 20")]
    [InlineData(15.123, "at most 2 decimal places")]
    public void ANumber_IsHeldToItsRangeAndDecimalPlaces(double number, string expected)
    {
        var cell = Cell(CellKind.Number, new NumberCellConfiguration { MinValue = 10, MaxValue = 20, DecimalPlaces = 2 });

        var problem = Assert.Throws<InvalidRequestException>(() => CellValueValidator.Validate(cell, Value(number: (decimal)number)));

        Assert.Contains(expected, problem.Message);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(15.25)]
    [InlineData(20)]
    public void ANumberInsideItsLimits_IsAccepted(double number)
    {
        var cell = Cell(CellKind.Number, new NumberCellConfiguration { MinValue = 10, MaxValue = 20, DecimalPlaces = 2 });

        CellValueValidator.Validate(cell, Value(number: (decimal)number));
    }

    [Theory]
    [InlineData(CellKind.TextDropdown)]
    [InlineData(CellKind.NumberDropdown)]
    public void ADropdown_OnlyTakesOneOfItsOwnOptions(CellKind kind)
    {
        var cell = Cell(kind, CellConfigurations.CreateEmpty(kind));
        cell.CellType.Options.Add(new CellTypeOption { Id = 3, Value = "1" });

        CellValueValidator.Validate(cell, Value(optionId: 3));
        Assert.Throws<InvalidRequestException>(() => CellValueValidator.Validate(cell, Value(optionId: 4)));
    }

    [Theory]
    [InlineData(CellKind.Text)]
    [InlineData(CellKind.Number)]
    [InlineData(CellKind.Date)]
    [InlineData(CellKind.Checkbox)]
    [InlineData(CellKind.TextDropdown)]
    public void ClearingACell_IsAlwaysAllowed(CellKind kind)
    {
        var cell = Cell(kind, CellConfigurations.CreateEmpty(kind));

        CellValueValidator.Validate(cell, Value());
    }

    [Fact]
    public void TheMessageNamesTheCellByItsCaption_OrItsTypeWhenItHasNone()
    {
        var cell = Cell(CellKind.Text, new TextCellConfiguration { MaxLength = 1 });
        var unnamed = Assert.Throws<InvalidRequestException>(() => CellValueValidator.Validate(cell, Value(text: "ab")));
        cell.Caption = "Bore";
        var named = Assert.Throws<InvalidRequestException>(() => CellValueValidator.Validate(cell, Value(text: "ab")));

        Assert.StartsWith("'Text type'", unnamed.Message);
        Assert.StartsWith("'Bore'", named.Message);
    }

    private static TemplateCell Cell(CellKind kind, CellConfiguration configuration)
    {
        return new TemplateCell
        {
            CellType = new CellType { Name = $"{kind} type", Kind = kind, Configuration = configuration },
        };
    }

    private static CellValueRequest Value(string? text = null, decimal? number = null, int? optionId = null)
    {
        return new CellValueRequest(1, text, number, null, null, optionId);
    }
}
