using PUSpecSheet.Api.Database.Schema.Template;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

public record DbSpecSheetTableWidget
{
    public required int Id { get; init; }

    public required int SpecSheetTableId { get; init; }

    public required int TableTemplateWidgetId { get; init; }

    public virtual DbTableTemplateWidget? TableTemplateWidget { get; set; }

    public virtual DbTableWidgetValue? Value { get; set; }
}
