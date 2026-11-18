using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A table widget's value in a table revision. The revision has the widget if it has this row; the value may be empty.
/// </summary>
public record DbSheetTableWidgetValue
{
    public required int SheetTableRevisionId { get; init; }

    public required int SheetTableWidgetId { get; init; }

    public required WidgetValue? Value { get; set; }

    public virtual DbSheetTableWidget? SheetTableWidget { get; init; }
}
