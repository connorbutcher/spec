using PUSpecSheet.Domain.CellTypes.LabelledText;

namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>
/// Settings for a labelled text cell: several short texts in one cell, such as a right-hand and a left-hand
/// part number, each shown with its label beside it. The sheet can change the slots cell by cell (see
/// <see cref="InstanceSettings.LabelledTextInstanceSettings"/>).
/// </summary>
public sealed record LabelledTextCellConfiguration : CellConfiguration
{
    /// <summary>The most characters each value can have.</summary>
    public int? MaxLength { get; init; }

    /// <summary>The values the cell holds, in display order.</summary>
    public IReadOnlyList<LabelledTextSlot>? Slots { get; init; }

    /// <summary>The colour of the labels, a hex value such as "#dc2626".</summary>
    public string? LabelColor { get; init; }

    public override CellConfiguration Apply(CellConfiguration? cellOverride)
    {
        if (cellOverride is not LabelledTextCellConfiguration labelled)
        {
            return this;
        }

        return new LabelledTextCellConfiguration
        {
            MaxLength = labelled.MaxLength ?? MaxLength,
            Slots = labelled.Slots ?? Slots,
            LabelColor = labelled.LabelColor ?? LabelColor,
        };
    }

    public override string? Validate()
    {
        if (MaxLength is < 1 or > TextCellConfiguration.LongestMaxLength)
        {
            return $"The max length must be between 1 and {TextCellConfiguration.LongestMaxLength}.";
        }

        if (!LabelledTextRules.IsColor(LabelColor))
        {
            return "The label colour must be a hex value such as #dc2626.";
        }

        return LabelledTextRules.CheckSlots(Slots);
    }

    /// <summary>Compares the slots item by item, which a record's own equality doesn't do for a list.</summary>
    public bool Equals(LabelledTextCellConfiguration? other)
    {
        return other is not null
            && MaxLength == other.MaxLength
            && LabelColor == other.LabelColor
            && LabelledTextRules.Same(Slots, other.Slots);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MaxLength, LabelColor, LabelledTextRules.HashOf(Slots));
    }

    protected override CellKind GetKind()
    {
        return CellKind.LabelledText;
    }
}
