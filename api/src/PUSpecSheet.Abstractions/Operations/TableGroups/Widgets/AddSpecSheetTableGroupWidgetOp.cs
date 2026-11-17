using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Abstractions.Operations.TableGroups.Widgets;

public sealed record AddSpecSheetTableGroupWidgetOp : IChangeOp
{
    public required int SpecSheetTableGroupId { get; init; }

    [Range(1, int.MaxValue)]
    public required int TableGroupTemplateWidgetId { get; init; }

    public required WidgetValue Value { get; init; }
}
