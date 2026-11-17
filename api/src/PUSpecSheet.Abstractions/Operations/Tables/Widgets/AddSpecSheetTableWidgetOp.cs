using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Abstractions.Operations.Tables.Widgets;

public sealed record AddSpecSheetTableWidgetOp : IChangeOp
{
    public required int SpecSheetTableId { get; init; }

    [Range(1, int.MaxValue)]
    public required int TableTemplateWidgetId { get; init; }

    public required WidgetValue Value { get; init; }
}
