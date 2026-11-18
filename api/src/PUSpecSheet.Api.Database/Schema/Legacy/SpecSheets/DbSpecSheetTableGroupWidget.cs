using PUSpecSheet.Api.Database.Schema.Template;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

public record DbSpecSheetTableGroupWidget
{
    public required int Id { get; init; }

    public required int SpecSheetTableGroupId { get; init; }

    public required int TableGroupTemplateWidgetId { get; init; }

    public virtual DbTableGroupTemplateWidget? TableGroupTemplateWidget { get; set; }

    public virtual DbTableGroupWidgetValue? Value { get; set; }
}
