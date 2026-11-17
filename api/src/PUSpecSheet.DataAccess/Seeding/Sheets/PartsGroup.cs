namespace PUSpecSheet.DataAccess.Seeding.Sheets;

public sealed record PartsGroup
{
    public required string Name { get; init; }

    public required (string Kind, string Characteristic, string Reference, decimal Base, decimal Step, decimal Spread)[] Rows { get; init; }
}
