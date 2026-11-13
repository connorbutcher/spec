using PUSpecSheet.Application.Phases.Queries;
using PUSpecSheet.Application.SheetTypes.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// The phases the showcase fills in: the V6 build and its A2 to A5 sub-phases. A missing one is added
/// (the sub-phases under V6), and each is given the Specification, Parts and PFKs sheet types. Phases that
/// already exist keep everything else they have.
/// </summary>
public sealed class ShowcasePhases
{
    public const string V6 = "V6";
    public const string A2 = "01-A2";
    public const string A3 = "A3";
    public const string A4 = "A4";
    public const string A5 = "A5";

    public const string SpecificationSheetType = "Specification";
    public const string PartsSheetType = "Parts";
    public const string PfksSheetType = "PFKs";

    /// <summary>Every showcase phase, the build first.</summary>
    public static readonly string[] Codes = [V6, A2, A3, A4, A5];

    private readonly PuSpecSheetDbContext db;

    public ShowcasePhases(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    /// <summary>The id of each showcase phase by code.</summary>
    public async Task<IReadOnlyDictionary<string, int>> EnsureAsync(CancellationToken cancellationToken)
    {
        var phases = await PhaseQueries.ListByCodesForEditAsync(db, Codes, cancellationToken);

        var build = phases.SingleOrDefault(phase => phase.Code == V6);
        if (build is null)
        {
            var order = await PhaseQueries.GetLastDisplayOrderAsync(db, null, cancellationToken) ?? 0;
            build = new DbPhase { Code = V6, Description = "V6 build", DisplayOrder = order + 1 };
            db.Phases.Add(build);
            phases.Add(build);
        }

        for (var index = 1; index < Codes.Length; index++)
        {
            if (phases.All(phase => phase.Code != Codes[index]))
            {
                var phase = new DbPhase { Code = Codes[index], DisplayOrder = index, ParentPhase = build };
                db.Phases.Add(phase);
                phases.Add(phase);
            }
        }

        string[] sheetTypeNames = [SpecificationSheetType, PartsSheetType, PfksSheetType];
        var sheetTypeIds = await SheetTypeQueries.ListIdsByNamesAsync(db, sheetTypeNames, cancellationToken);
        foreach (var phase in phases)
        {
            foreach (var sheetTypeId in sheetTypeIds.Where(id => phase.SheetTypes.All(link => link.SheetTypeId != id)))
            {
                phase.SheetTypes.Add(new DbPhaseSheetType { SheetTypeId = sheetTypeId });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return phases.ToDictionary(phase => phase.Code, phase => phase.Id);
    }
}
