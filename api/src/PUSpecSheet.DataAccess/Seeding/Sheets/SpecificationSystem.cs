namespace PUSpecSheet.DataAccess.Seeding.Sheets;

/// <summary>One system of the sample specification and the limits under it, as on the V6 sheet.</summary>
public sealed record SpecificationSystem
{
    public required string Name { get; init; }

    public required (string Name, decimal Nominal, decimal Min, decimal Max)[] Dimensions { get; init; }

    public required (string Component, string Material)[] Materials { get; init; }

    public required (string Fastener, int Thread, decimal Torque, int Angle)[] Joints { get; init; }

    public required (string Surface, decimal Ra)[] Finishes { get; init; }
}
