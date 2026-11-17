using PUSpecSheet.DataAccess.Seeding.ReferenceData;

namespace PUSpecSheet.DataAccess.Seeding;

/// <summary>What the showcase sheets are built from: the cell types, the phases by code and the templates by name.</summary>
public sealed record ShowcaseContext
{
    public required ShowcaseCellTypeCatalog CellTypes { get; init; }

    public required IReadOnlyDictionary<string, int> PhaseIds { get; init; }

    public required IReadOnlyDictionary<string, int> TemplateIds { get; init; }

    /// <summary>How far along the build a phase is, 0 for V6: the sample values drift a little with each one.</summary>
    public static int Step(string phaseCode)
    {
        return Array.IndexOf(ShowcasePhases.Codes, phaseCode);
    }
}
