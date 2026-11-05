namespace PUSpecSheet.Application.Seeding;

/// <summary>What the showcase sheets are built from: the cell types, the phases by code and the templates by name.</summary>
public sealed record ShowcaseContext(
    ShowcaseCellTypeCatalog CellTypes,
    IReadOnlyDictionary<string, int> PhaseIds,
    IReadOnlyDictionary<string, int> TemplateIds)
{
    /// <summary>How far along the build a phase is, 0 for V6: the sample values drift a little with each one.</summary>
    public static int Step(string phaseCode)
    {
        return Array.IndexOf(ShowcasePhases.Codes, phaseCode);
    }
}
