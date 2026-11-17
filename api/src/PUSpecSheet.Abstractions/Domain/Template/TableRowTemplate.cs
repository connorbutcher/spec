using PUSpecSheet.Abstractions.Domain.Configuration;

namespace PUSpecSheet.Abstractions.Domain.Template;

public record TableRowTemplate
{
    public required int Id { get; init; }

    public required int TableGroupTemplateId { get; init; }

    public required int RowNumber { get; init; }

    public required bool ShowContextMenu { get; init; }

    public required bool CanBeReordered { get; init; }

    public virtual ICollection<TableCellTemplate> CellTemplates { get; init; } = [];

    public required ICollection<Validator>? Validators { get; init; }
}
