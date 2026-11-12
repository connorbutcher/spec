namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// One group of the sample parts matrix. Each row is a kind of section of the template, what it is called,
/// its drawing reference, the baseline value, how much it changes from one part to the next and, for a
/// dimension, the allowed spread either side.
/// </summary>
public sealed record PartsGroup
{
    public PartsGroup(
        string name,
        (string Kind, string Characteristic, string Reference, decimal Base, decimal Step, decimal Spread)[] rows)
    {
        this.Name = name;
        this.Rows = rows;
    }

    public string Name { get; init; }

    public (string Kind, string Characteristic, string Reference, decimal Base, decimal Step, decimal Spread)[] Rows { get; init; }
}
