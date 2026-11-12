namespace PUSpecSheet.Application.Seeding;

/// <summary>One system of the sample specification and the limits under it, as on the V6 sheet.</summary>
public sealed record SpecificationSystem
{
    public SpecificationSystem(
        string name,
        (string Name, decimal Nominal, decimal Min, decimal Max)[] dimensions,
        (string Component, string Material)[] materials,
        (string Fastener, int Thread, decimal Torque, int Angle)[] joints,
        (string Surface, decimal Ra)[] finishes)
    {
        this.Name = name;
        this.Dimensions = dimensions;
        this.Materials = materials;
        this.Joints = joints;
        this.Finishes = finishes;
    }

    public string Name { get; init; }

    public (string Name, decimal Nominal, decimal Min, decimal Max)[] Dimensions { get; init; }

    public (string Component, string Material)[] Materials { get; init; }

    public (string Fastener, int Thread, decimal Torque, int Angle)[] Joints { get; init; }

    public (string Surface, decimal Ra)[] Finishes { get; init; }
}
