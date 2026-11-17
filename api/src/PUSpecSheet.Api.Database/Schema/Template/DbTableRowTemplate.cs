using PUSpecSheet.Abstractions.Domain.Configuration;

namespace PUSpecSheet.Api.Database.Schema.Template;

public record DbTableRowTemplate
{
    public required int Id { get; init; }

    public required int TableGroupTemplateId { get; init; }

    public required int RowNumber { get; init; }

    public required bool ShowContextMenu { get; init; }

    public required bool CanBeReordered { get; init; }

    public virtual ICollection<DbTableCellTemplate> CellTemplates { get; init; } = [];

    public ICollection<Validator>? Validators { get; init; } = [];
}
