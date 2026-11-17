using PUSpecSheet.Abstractions.Domain.Properties.Cell;

namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// A cell's content while its values are being read, one table at a time.
/// </summary>
internal sealed class CellContentParts
{
    public string? Text { get; set; }

    public double? Number { get; set; }

    public string? PairX { get; set; }

    public string? PairY { get; set; }

    public LinkedSheetTarget? Linked { get; set; }

    public ICellProperties? ConfigurationOverride { get; set; }

    public CellContent Build()
    {
        return new CellContent
        {
            Text = Text,
            Number = Number,
            PairX = PairX,
            PairY = PairY,
            Linked = Linked,
            ConfigurationOverride = ConfigurationOverride
        };
    }
}
