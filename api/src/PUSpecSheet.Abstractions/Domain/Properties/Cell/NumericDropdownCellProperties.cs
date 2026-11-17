namespace PUSpecSheet.Abstractions.Domain.Properties.Cell;

public sealed class NumericDropdownCellProperties : ICellProperties
{
    public required ICollection<double> Options { get; init; }

    public double? DefaultValue { get; set; }
}
