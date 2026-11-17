using PUSpecSheet.Abstractions.Domain.Properties.Table;

namespace PUSpecSheet.Abstractions.Domain.Template;

public record TableTemplate
{
    public required int Id { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required string Type { get; init; }

    public required string Description { get; init; }

    public required bool RequiresUserConfiguration { get; init; }

    public required bool ShowNotes { get; init; }

    public required bool IsLegacy { get; init; }

    public required ITableProperties? DefaultConfiguration { get; init; }

    public virtual ICollection<TableGroupTemplate> TableGroupTemplates { get; init; } = [];

    public virtual ICollection<TableTemplateWidget> TableTemplateWidgets { get; init; } = [];
}
