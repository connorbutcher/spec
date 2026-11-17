using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.Domain.Common;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Abstractions.Operations.TableGroups.Widgets;

public sealed record UpdateSpecSheetTableGroupWidgetOp : IChangeOp
{
    [Range(1, int.MaxValue)]
    public required int Id { get; init; }

    public required PropertyChanged<WidgetValue?>? WidgetValue { get; init; }
}
