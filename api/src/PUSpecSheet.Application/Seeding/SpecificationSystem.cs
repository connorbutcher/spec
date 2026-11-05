namespace PUSpecSheet.Application.Seeding;

/// <summary>One system of the sample specification and the limits under it, as on the V6 sheet.</summary>
public sealed record SpecificationSystem(
    string Name,
    (string Name, decimal Nominal, decimal Min, decimal Max)[] Dimensions,
    (string Component, string Material)[] Materials,
    (string Fastener, int Thread, decimal Torque, int Angle)[] Joints,
    (string Surface, decimal Ra)[] Finishes);
