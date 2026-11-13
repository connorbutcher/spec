using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>A cell's content while its values are read from the separate value tables, one kind at a time.</summary>
internal sealed class CellContentParts
{
    public string? Text { get; set; }

    public decimal? Number { get; set; }

    public DateOnly? Date { get; set; }

    public bool? Boolean { get; set; }

    public int? OptionId { get; set; }

    public List<LabelledSlot> LabelledText { get; } = [];

    public CellInstanceSettings? Settings { get; set; }

    public CellContent Build()
    {
        return new CellContent(Text, Number, Date, Boolean, OptionId, LabelledText, Settings);
    }
}
