using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Phases;

/// <summary>The full list of sheet types a phase has. Types left out are removed from the phase.</summary>
public sealed record SetPhaseSheetTypesRequest
{
    public SetPhaseSheetTypesRequest([Required] IReadOnlyList<int> sheetTypeIds)
    {
        SheetTypeIds = sheetTypeIds;
    }

    public IReadOnlyList<int> SheetTypeIds { get; init; }
}
