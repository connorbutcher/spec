using PUSpecSheet.Abstractions.Domain.Properties.Table;

namespace PUSpecSheet.Api.Database.Schema.Template;

public record DbTableTemplate
{
    public required int Id { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required string Type { get; init; }

    public required string Description { get; init; }

    public required bool RequiresUserConfiguration { get; init; }

    public required bool ShowNotes { get; init; }

    public required bool IsLegacy { get; init; }

    public ITableProperties? DefaultConfiguration { get; init; }

    public virtual ICollection<DbTableGroupTemplate> TableGroupTemplates { get; init; } = [];

    public virtual ICollection<DbTableTemplateWidget> TableTemplateWidgets { get; init; } = [];
}
