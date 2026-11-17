namespace PUSpecSheet.Abstractions.Domain.Properties.Cell;

public sealed class TextDropdownCellProperties : ICellProperties
{
    public required ICollection<string> Options { get; init; }

    public string? DefaultValue { get; set; }

}
