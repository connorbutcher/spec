namespace PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

public record PartPairWidgetValue : WidgetValue
{
    public required string TopValue { get; set; }

    public required string BottomValue { get; set; }
}
