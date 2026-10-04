using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A cell on a sheet: the layout it was built from (<see cref="Template"/>, whose own id is the template
/// cell's) and its value. Only the value field that matches the cell's kind is ever set.
/// </summary>
public sealed record SheetCellDto(
    int Id,
    Guid PublicId,
    TemplateCellDto Template,
    string? TextValue,
    decimal? NumberValue,
    DateOnly? DateValue,
    bool? BooleanValue,
    int? OptionId,
    int? SheetColumnBlockId,
    SheetChangeDto? LastChange);
