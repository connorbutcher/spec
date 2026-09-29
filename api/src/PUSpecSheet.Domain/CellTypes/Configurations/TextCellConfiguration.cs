namespace PUSpecSheet.Domain.CellTypes.Configurations;

/// <summary>Settings for a free text cell.</summary>
public sealed record TextCellConfiguration : CellConfiguration
{
    public const int LongestMaxLength = 4000;

    protected override CellKind GetKind()
    {
        return CellKind.Text;
    }

    /// <summary>The most characters a value can have.</summary>
    public int? MaxLength { get; init; }

    /// <summary>Whether the value is entered in a multi-line box.</summary>
    public bool? Multiline { get; init; }

    public override CellConfiguration Apply(CellConfiguration? overrides)
    {
        if (overrides is not TextCellConfiguration text)
        {
            return this;
        }

        return new TextCellConfiguration
        {
            MaxLength = text.MaxLength ?? MaxLength,
            Multiline = text.Multiline ?? Multiline,
        };
    }

    public override string? Validate()
    {
        if (MaxLength is < 1 or > LongestMaxLength)
        {
            return $"The max length must be between 1 and {LongestMaxLength}.";
        }

        return null;
    }
}
