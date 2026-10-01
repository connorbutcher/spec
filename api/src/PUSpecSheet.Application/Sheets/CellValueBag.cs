namespace PUSpecSheet.Application.Sheets;

/// <summary>One cell's value, gathered from the typed value tables. Only the field for its kind is set.</summary>
public sealed class CellValueBag
{
    public string? Text { get; set; }

    public decimal? Number { get; set; }

    public DateOnly? Date { get; set; }

    public bool? Boolean { get; set; }

    public int? OptionId { get; set; }
}
