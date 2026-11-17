using PUSpecSheet.DataAccess.Seeding.ReferenceData;

namespace PUSpecSheet.DataAccess.Seeding;

public sealed record ShowcaseContext
{
    public required ShowcaseCellTypeCatalog CellTypes { get; init; }

    public required IReadOnlyDictionary<string, int> PhaseIds { get; init; }

    public required IReadOnlyDictionary<string, int> TemplateIds { get; init; }

    public static int Step(string phaseCode)
    {
        return Array.IndexOf(ShowcasePhases.Codes, phaseCode);
    }
}
