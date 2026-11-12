namespace PUSpecSheet.Domain.CellTypes.LabelledText;

/// <summary>
/// One of the values a labelled text cell holds, such as the right-hand part number. <see cref="Key"/> ties
/// the value to the slot and never changes; <see cref="Label"/> is what is shown beside it, such as "RH".
/// </summary>
public sealed record LabelledTextSlot
{
    public LabelledTextSlot(string key, string label)
    {
        this.Key = key;
        this.Label = label;
    }

    public string Key { get; init; }

    public string Label { get; init; }
}
