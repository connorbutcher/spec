namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>
/// One group of the sample parts matrix. Each row is a kind of section of the template, what it is called,
/// its drawing reference, the baseline value, how much it changes from one part to the next and, for a
/// dimension, the allowed spread either side.
/// </summary>
public sealed record PartsGroup
{
    public required string Name { get; init; }

    public required (string Kind, string Characteristic, string Reference, decimal Base, decimal Step, decimal Spread)[] Rows { get; init; }
}
