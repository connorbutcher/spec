using PUSpecSheet.DataAccess.Phases;
using PUSpecSheet.DataAccess.SheetTypes;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.DataAccess.Seeding.ReferenceData;

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

    public const string EngineSpecificationsSheetType = "Engine Specifications";

    public static readonly string[] Codes = [V6, A2, A3, A4, A5];

    private readonly IPhaseDataService _phaseData;
    private readonly ISheetTypeDataService _sheetTypes;

    public ShowcasePhases(IPhaseDataService phaseData, ISheetTypeDataService sheetTypes)
    {
        _phaseData = phaseData;
        _sheetTypes = sheetTypes;
    }

    public async Task<IReadOnlyDictionary<string, int>> EnsureAsync(CancellationToken cancellationToken)
    {
        var phases = await _phaseData.ListByCodesForEditAsync(Codes, cancellationToken);

        var build = phases.SingleOrDefault(phase => phase.Code == V6);
        if (build is null)
        {
            var order = await _phaseData.GetLastDisplayOrderAsync(null, cancellationToken) ?? 0;
            build = new DbPhase { Code = V6, Description = "V6 build", DisplayOrder = order + 1 };
            _phaseData.Add(build);
            phases.Add(build);
        }

        for (var index = 1; index < Codes.Length; index++)
        {
            if (phases.All(phase => phase.Code != Codes[index]))
            {
                var phase = new DbPhase { Code = Codes[index], DisplayOrder = index, ParentPhase = build };
                _phaseData.Add(phase);
                phases.Add(phase);
            }
        }

        string[] sheetTypeNames = [SpecificationSheetType, PartsSheetType, PfksSheetType];
        var sheetTypeIds = await _sheetTypes.ListIdsByNamesAsync(sheetTypeNames, cancellationToken);
        foreach (var phase in phases)
        {
            foreach (var sheetTypeId in sheetTypeIds.Where(id => phase.SheetTypes.All(link => link.SheetTypeId != id)))
            {
                phase.SheetTypes.Add(new DbPhaseSheetType { SheetTypeId = sheetTypeId });
            }
        }

        var engineSpecificationsId = await _sheetTypes.GetIdByNameAsync(EngineSpecificationsSheetType, cancellationToken);
        if (build.SheetTypes.All(link => link.SheetTypeId != engineSpecificationsId))
        {
            build.SheetTypes.Add(new DbPhaseSheetType { SheetTypeId = engineSpecificationsId });
        }

        await _phaseData.SaveAsync(cancellationToken);
        return phases.ToDictionary(phase => phase.Code, phase => phase.Id);
    }
}
