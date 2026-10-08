using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets;

/// <summary>One cell's value, gathered from the typed value tables. Only the value field for its kind is set.</summary>
public sealed class CellValueBag
{
    public string? Text { get; set; }

    public decimal? Number { get; set; }

    public DateOnly? Date { get; set; }

    public bool? Boolean { get; set; }

    public int? OptionId { get; set; }

    /// <summary>Whether the cell holds a value. Settings alone don't count: a linked dropdown with nothing chosen is empty.</summary>
    public bool HasValue => Text is not null || Number is not null || Date is not null || Boolean is not null || OptionId is not null;

    /// <summary>The settings chosen for the cell on the sheet, for a kind that has them. Not a value: a cell can have both.</summary>
    public CellInstanceSettings? Settings { get; set; }
}
