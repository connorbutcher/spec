namespace PUSpecSheet.DataAccess.Seeding.Sheets;

public sealed record SpecificationSystem
{
    public required string Name { get; init; }

    public required (string Name, decimal Nominal, decimal Min, decimal Max)[] Dimensions { get; init; }

    public required (string Component, string Material)[] Materials { get; init; }

    public required (string Fastener, int Thread, decimal Torque, int Angle)[] Joints { get; init; }

    public required (string Surface, decimal Ra)[] Finishes { get; init; }
}
