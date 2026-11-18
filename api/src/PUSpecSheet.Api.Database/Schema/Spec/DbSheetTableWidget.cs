using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Schema.Spec;

/// <summary>
/// A widget of a sheet table, one for each template widget the table has. Its value in each table revision is a <see cref="DbSheetTableWidgetValue"/>.
/// </summary>
public record DbSheetTableWidget
{
    public required int Id { get; init; }

    public required int SheetTableId { get; init; }

    public required int TableTemplateWidgetId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbTableTemplateWidget? TableTemplateWidget { get; init; }
}
