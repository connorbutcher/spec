using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A table group widget's value in a group revision. The revision has the widget if it has this row; the value may be empty.
/// </summary>
public record DbSheetTableGroupWidgetValue
{
    public required int SheetTableGroupRevisionId { get; init; }

    public required int SheetTableGroupWidgetId { get; init; }

    public required WidgetValue? Value { get; set; }

    public virtual DbSheetTableGroupWidget? SheetTableGroupWidget { get; init; }
}
