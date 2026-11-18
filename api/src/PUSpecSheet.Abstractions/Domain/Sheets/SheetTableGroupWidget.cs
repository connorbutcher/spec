using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A widget of a group and its value in the group revision that is shown.
/// </summary>
public record SheetTableGroupWidget
{
    public required int Id { get; init; }

    public required int SheetTableGroupId { get; init; }

    public required int TableGroupTemplateWidgetId { get; init; }

    public required WidgetValue? Value { get; init; }
}
