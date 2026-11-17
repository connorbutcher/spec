using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;

namespace PUSpecSheet.Migrator.Snapshots;

internal sealed class CellContentParts
{
    public string? Text { get; set; }

    public decimal? Number { get; set; }

    public DateOnly? Date { get; set; }

    public bool? Boolean { get; set; }

    public int? OptionId { get; set; }

    public List<LabelledSlot> LabelledText { get; } = [];

    public CellInstanceSettings? Settings { get; set; }

    public LinkedSheetTarget? Linked { get; set; }

    public CellContent Build()
    {
        return new CellContent(Text, Number, Date, Boolean, OptionId, LabelledText, Settings, Linked);
    }
}
