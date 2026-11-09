using PUSpecSheet.Domain.CellTypes.LabelledText;

namespace PUSpecSheet.Domain.CellTypes.InstanceSettings;

/// <summary>
/// The slots one labelled text cell has on the sheet, when they differ from its template's: other labels,
/// more values or fewer. Null keeps the template's.
/// </summary>
public sealed record LabelledTextInstanceSettings : CellInstanceSettings
{
    public IReadOnlyList<LabelledTextSlot>? Slots { get; init; }

    /// <summary>Compares the slots item by item, which a record's own equality doesn't do for a list.</summary>
    public bool Equals(LabelledTextInstanceSettings? other)
    {
        return other is not null && LabelledTextRules.Same(Slots, other.Slots);
    }

    public override int GetHashCode()
    {
        return LabelledTextRules.HashOf(Slots);
    }

    protected override CellKind GetKind()
    {
        return CellKind.LabelledText;
    }
}
