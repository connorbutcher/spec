using PUSpecSheet.DataAccess.Phases;
using PUSpecSheet.DataAccess.SheetTypes;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.DataAccess.Seeding.ReferenceData;

/// <summary>
/// The phases the showcase fills in: the V6 build and its A2 to A5 sub-phases. A missing one is added
/// (the sub-phases under V6), and each is given the Specification, Parts and PFKs sheet types, and V6 the
/// Engine Specifications one too. Phases that already exist keep everything else they have.
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

    /// <summary>The one showcase sheet type with many sheets in a phase, told apart by serial number. Only V6 has it.</summary>
    public const string EngineSpecificationsSheetType = "Engine Specifications";

    /// <summary>Every showcase phase, the build first.</summary>
    public static readonly string[] Codes = [V6, A2, A3, A4, A5];

    private readonly IPhaseDataService phaseData;
    private readonly ISheetTypeDataService sheetTypes;

    public ShowcasePhases(IPhaseDataService phaseData, ISheetTypeDataService sheetTypes)
    {
        this.phaseData = phaseData;
        this.sheetTypes = sheetTypes;
    }

    /// <summary>The id of each showcase phase by code.</summary>
    public async Task<IReadOnlyDictionary<string, int>> EnsureAsync(CancellationToken cancellationToken)
    {
        var phases = await phaseData.ListByCodesForEditAsync(Codes, cancellationToken);

        var build = phases.SingleOrDefault(phase => phase.Code == V6);
        if (build is null)
        {
            var order = await phaseData.GetLastDisplayOrderAsync(null, cancellationToken) ?? 0;
            build = new DbPhase { Code = V6, Description = "V6 build", DisplayOrder = order + 1 };
            phaseData.Add(build);
            phases.Add(build);
        }

        for (var index = 1; index < Codes.Length; index++)
        {
            if (phases.All(phase => phase.Code != Codes[index]))
            {
                var phase = new DbPhase { Code = Codes[index], DisplayOrder = index, ParentPhase = build };
                phaseData.Add(phase);
                phases.Add(phase);
            }
        }

        string[] sheetTypeNames = [SpecificationSheetType, PartsSheetType, PfksSheetType];
        var sheetTypeIds = await sheetTypes.ListIdsByNamesAsync(sheetTypeNames, cancellationToken);
        foreach (var phase in phases)
        {
            foreach (var sheetTypeId in sheetTypeIds.Where(id => phase.SheetTypes.All(link => link.SheetTypeId != id)))
            {
                phase.SheetTypes.Add(new DbPhaseSheetType { SheetTypeId = sheetTypeId });
            }
        }

        var engineSpecificationsId = await sheetTypes.GetIdByNameAsync(EngineSpecificationsSheetType, cancellationToken);
        if (build.SheetTypes.All(link => link.SheetTypeId != engineSpecificationsId))
        {
            build.SheetTypes.Add(new DbPhaseSheetType { SheetTypeId = engineSpecificationsId });
        }

        await phaseData.SaveAsync(cancellationToken);
        return phases.ToDictionary(phase => phase.Code, phase => phase.Id);
    }
}
