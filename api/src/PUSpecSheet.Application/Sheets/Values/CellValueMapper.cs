using System.Globalization;
using System.Text.Json;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>
/// Turns a stored cell value into the single value a cell carries in the API, and a value sent for a cell
/// back into what is stored, by the cell's kind (see <see cref="SheetCellDto.Value"/>).
/// </summary>
public static class CellValueMapper
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>The value a cell carries: whichever the cell's kind stores, or null when it holds none.</summary>
    public static object? ToValue(CellValueBag? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Text is not null)
        {
            return value.Text;
        }

        if (value.Number is { } number)
        {
            return number;
        }

        if (value.Date is { } date)
        {
            return date.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        if (value.Boolean is { } flag)
        {
            return flag;
        }

        if (value.OptionId is { } optionId)
        {
            return optionId;
        }

        return value.Slots is { Count: > 0 } slots ? slots : null;
    }

    /// <summary>The value sent for a cell, read the way the cell's kind stores it.</summary>
    /// <param name="kind">The cell's kind.</param>
    /// <param name="value">The value as sent; null or JSON null clears the cell.</param>
    /// <param name="name">What the cell is called, for the message when the value has the wrong shape.</param>
    /// <exception cref="InvalidRequestException">The value isn't in the shape the cell's kind takes.</exception>
    public static CellValueBag FromRequest(CellKind kind, JsonElement? value, string name)
    {
        var bag = new CellValueBag();
        if (value is not { } element || element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined || !kind.StoresValue())
        {
            return bag;
        }

        switch (kind)
        {
            case CellKind.Text:
            case CellKind.LinkedDropdown:
                bag.Text = element.ValueKind == JsonValueKind.String
                    ? element.GetString()
                    : throw WrongShape(name, "text");
                break;

            case CellKind.Number:
                bag.Number = element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var number)
                    ? number
                    : throw WrongShape(name, "a number");
                break;

            case CellKind.Date:
                bag.Date = element.ValueKind == JsonValueKind.String
                    && DateOnly.TryParseExact(element.GetString(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                    ? date
                    : throw WrongShape(name, $"a date written {DateFormat}");
                break;

            case CellKind.Checkbox:
                bag.Boolean = element.ValueKind is JsonValueKind.True or JsonValueKind.False
                    ? element.GetBoolean()
                    : throw WrongShape(name, "true or false");
                break;

            case CellKind.TextDropdown:
            case CellKind.NumberDropdown:
                bag.OptionId = element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var optionId)
                    ? optionId
                    : throw WrongShape(name, "the id of one of its choices");
                break;

            case CellKind.LabelledText:
                bag.Slots = SlotsOf(element, name);
                break;
        }

        return bag;
    }

    private static Dictionary<string, string> SlotsOf(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw WrongShape(name, "an object of values by slot");
        }

        var slots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var slot in element.EnumerateObject())
        {
            if (slot.Value.ValueKind == JsonValueKind.Null)
            {
                continue;
            }

            slots[slot.Name] = slot.Value.ValueKind == JsonValueKind.String
                ? slot.Value.GetString()!
                : throw WrongShape(name, "text in each slot");
        }

        return slots;
    }

    private static InvalidRequestException WrongShape(string name, string expected)
    {
        return new InvalidRequestException($"'{name}' takes {expected}.");
    }
}
