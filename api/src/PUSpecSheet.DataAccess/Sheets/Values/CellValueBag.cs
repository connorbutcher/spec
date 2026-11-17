using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;

namespace PUSpecSheet.DataAccess.Sheets.Values;

public sealed class CellValueBag
{
    public string? Text { get; set; }

    public decimal? Number { get; set; }

    public DateOnly? Date { get; set; }

    public bool? Boolean { get; set; }

    public int? OptionId { get; set; }

    public Dictionary<string, string>? Slots { get; set; }

    public bool HasValue => Text is not null || Number is not null || Date is not null || Boolean is not null || OptionId is not null || Slots is { Count: > 0 };

    public CellInstanceSettings? Settings { get; set; }
}
