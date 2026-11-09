using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Domain.CellTypes.LabelledText;

/// <summary>Works out which values a labelled text cell holds on a sheet.</summary>
public static class LabelledTextSlots
{
    /// <summary>
    /// The slots chosen for the cell on the sheet, or else its template's (cell type defaults with the cell's
    /// overrides on top), or else one unlabelled value.
    /// </summary>
    public static IReadOnlyList<LabelledTextSlot> Effective(CellConfiguration? configuration, CellInstanceSettings? settings)
    {
        return (settings as LabelledTextInstanceSettings)?.Slots
            ?? (configuration as LabelledTextCellConfiguration)?.Slots
            ?? LabelledTextRules.DefaultSlots;
    }
}
