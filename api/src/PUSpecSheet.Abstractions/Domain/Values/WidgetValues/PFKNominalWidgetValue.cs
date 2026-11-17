namespace PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

public record PFKNominalWidgetValue : WidgetValue
{
    public required bool AutomaticNominalCalculation { get; init; }
}
