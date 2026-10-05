using System.Globalization;
using System.Text.Json;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Tests.Published;

/// <summary>Values reach a caller as plain JSON, without the padding the database adds.</summary>
public sealed class PublishedValuesTests
{
    [Theory]
    [InlineData("12.5000000000", "12.5")]
    [InlineData("100.0000000000", "100")]
    [InlineData("0.0000000000", "0")]
    [InlineData("-0.0250000000", "-0.025")]
    [InlineData("123456789012345678.1234567890", "123456789012345678.123456789")]
    public void ANumber_LosesItsTrailingZeros(string stored, string sent)
    {
        var value = PublishedValues.Number(decimal.Parse(stored, CultureInfo.InvariantCulture));

        Assert.Equal(sent, value.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ANumberDropdown_GivesANumberAndATextDropdownGivesText()
    {
        Assert.Equal(1.5m, PublishedValues.Option("1.50", CellKind.NumberDropdown));
        Assert.Equal("1.50", PublishedValues.Option("1.50", CellKind.TextDropdown));
        Assert.Equal("n/a", PublishedValues.Option("n/a", CellKind.NumberDropdown));
    }

    [Fact]
    public void AFlatSheet_IsSentAsOneMapOfPlainValues()
    {
        var cells = new Dictionary<Guid, object>
        {
            [Guid.Parse("a41e0000-0000-0000-0000-000000000001")] = PublishedValues.Number(12.5000000000m),
            [Guid.Parse("a41e0000-0000-0000-0000-000000000002")] = "M8",
            [Guid.Parse("a41e0000-0000-0000-0000-000000000003")] = true,
            [Guid.Parse("a41e0000-0000-0000-0000-000000000004")] = new DateOnly(2026, 1, 2),
        };
        var sheet = new PublishedSheetDto(
            Guid.Parse("6f1c0000-0000-0000-0000-000000000001"),
            7,
            new DateTime(2026, 9, 30, 14, 2, 11, DateTimeKind.Utc),
            null,
            cells,
            []);

        var json = JsonSerializer.Serialize(sheet, JsonSerializerOptions.Web);

        Assert.Equal(
            "{\"sheet\":\"6f1c0000-0000-0000-0000-000000000001\",\"version\":7,\"publishedAtUtc\":\"2026-09-30T14:02:11Z\","
            + "\"cells\":{\"a41e0000-0000-0000-0000-000000000001\":12.5,\"a41e0000-0000-0000-0000-000000000002\":\"M8\","
            + "\"a41e0000-0000-0000-0000-000000000003\":true,\"a41e0000-0000-0000-0000-000000000004\":\"2026-01-02\"},\"missing\":[]}",
            json);
    }

    [Fact]
    public void ATreeCell_IsSentWithoutEmptyFields()
    {
        var cell = new PublishedCellDto(Guid.Parse("a41e0000-0000-0000-0000-000000000001"), 12.5m, null, null);

        var json = JsonSerializer.Serialize(cell, JsonSerializerOptions.Web);

        Assert.Equal("{\"id\":\"a41e0000-0000-0000-0000-000000000001\",\"value\":12.5}", json);
    }
}
