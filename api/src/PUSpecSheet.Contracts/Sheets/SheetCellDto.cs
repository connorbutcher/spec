using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A cell on a sheet: the template cell it was built from (<see cref="TemplateCellId"/>, found in the
/// sheet's <see cref="SheetDto.TemplateCells"/>, which lists each one once) and its value. Only the value
/// field that matches the cell's kind is ever set.
/// </summary>
public sealed record SheetCellDto(
    int Id,
    Guid PublicId,
    int TemplateCellId,
    string? TextValue,
    decimal? NumberValue,
    DateOnly? DateValue,
    bool? BooleanValue,
    int? OptionId,
    int? SheetColumnBlockId,
    SheetChangeDto? LastChange)
{
    /// <summary>
    /// The settings chosen for this cell on the sheet, for a kind that has them (where a linked dropdown
    /// takes its choices from, for example); null when nothing has been chosen.
    /// </summary>
    public CellInstanceSettings? Settings { get; init; }

    /// <summary>A labelled text cell's values by slot key, in the order they were saved; null when it holds none.</summary>
    public IReadOnlyDictionary<string, string>? Slots { get; init; }
}
