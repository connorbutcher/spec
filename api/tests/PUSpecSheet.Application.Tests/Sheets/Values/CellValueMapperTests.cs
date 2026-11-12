using System.Text.Json;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Tests.Sheets.Values;

/// <summary>A cell carries one value in the API, in the shape its kind takes, and a value sent back is read the same way.</summary>
public sealed class CellValueMapperTests
{
    [Fact]
    public void EachKindsValue_IsCarriedAsItsOwnShape()
    {
        Assert.Equal("Bolt", CellValueMapper.ToValue(new CellValueBag { Text = "Bolt" }));
        Assert.Equal(1.5m, CellValueMapper.ToValue(new CellValueBag { Number = 1.5m }));
        Assert.Equal("2026-11-12", CellValueMapper.ToValue(new CellValueBag { Date = new DateOnly(2026, 11, 12) }));
        Assert.Equal(true, CellValueMapper.ToValue(new CellValueBag { Boolean = true }));
        Assert.Equal(7, CellValueMapper.ToValue(new CellValueBag { OptionId = 7 }));
    }

    [Fact]
    public void ACellWithNothing_CarriesNull()
    {
        Assert.Null(CellValueMapper.ToValue(null));
        Assert.Null(CellValueMapper.ToValue(new CellValueBag { Slots = [] }));
    }

    [Fact]
    public void Settings_AreNotAValue()
    {
        Assert.Null(CellValueMapper.ToValue(new CellValueBag()));
    }

    [Fact]
    public void ValuesSent_AreReadByTheCellsKind()
    {
        Assert.Equal("Bolt", CellValueMapper.FromRequest(CellKind.Text, Json("\"Bolt\""), "Name").Text);
        Assert.Equal(1.5m, CellValueMapper.FromRequest(CellKind.Number, Json("1.5"), "Torque").Number);
        Assert.Equal(new DateOnly(2026, 11, 12), CellValueMapper.FromRequest(CellKind.Date, Json("\"2026-11-12\""), "Due").Date);
        Assert.True(CellValueMapper.FromRequest(CellKind.Checkbox, Json("true"), "Done").Boolean);
        Assert.Equal(7, CellValueMapper.FromRequest(CellKind.NumberDropdown, Json("7"), "Size").OptionId);
        Assert.Equal(
            new Dictionary<string, string> { ["rh"] = "A1", ["lh"] = "A2" },
            CellValueMapper.FromRequest(CellKind.LabelledText, Json("{\"rh\":\"A1\",\"lh\":\"A2\",\"x\":null}"), "Part").Slots);
    }

    [Theory]
    [InlineData(CellKind.Text)]
    [InlineData(CellKind.Number)]
    [InlineData(CellKind.LabelledText)]
    public void NullOrNothing_ClearsTheCell(CellKind kind)
    {
        Assert.False(CellValueMapper.FromRequest(kind, null, "Cell").HasValue);
        Assert.False(CellValueMapper.FromRequest(kind, Json("null"), "Cell").HasValue);
    }

    [Theory]
    [InlineData(CellKind.Text, "12")]
    [InlineData(CellKind.Number, "\"12\"")]
    [InlineData(CellKind.Date, "\"12/11/2026\"")]
    [InlineData(CellKind.Checkbox, "1")]
    [InlineData(CellKind.TextDropdown, "1.5")]
    [InlineData(CellKind.LabelledText, "[\"A1\"]")]
    public void AValueOfTheWrongShape_IsRefused(CellKind kind, string json)
    {
        var problem = Assert.Throws<InvalidRequestException>(() => CellValueMapper.FromRequest(kind, Json(json), "Cell"));

        Assert.StartsWith("'Cell' takes", problem.Message);
    }

    private static JsonElement Json(string json)
    {
        return JsonDocument.Parse(json).RootElement.Clone();
    }
}
