namespace PUSpecSheet.Abstractions.Domain.Properties.Cell;

public sealed class NumberCellProperties : ICellProperties
{
    public required int DecimalPlaces { get; set; }

    public double? DefaultValue { get; set; }
}
