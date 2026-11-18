using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A widget of a sheet table group, one for each template widget the group has. Its value in each group revision is a <see cref="DbSheetTableGroupWidgetValue"/>.
/// </summary>
public record DbSheetTableGroupWidget
{
    public required int Id { get; init; }

    public required int SheetTableGroupId { get; init; }

    public required int TableGroupTemplateWidgetId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbTableGroupTemplateWidget? TableGroupTemplateWidget { get; init; }
}
