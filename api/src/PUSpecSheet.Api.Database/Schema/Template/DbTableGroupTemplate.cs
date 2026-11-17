namespace PUSpecSheet.Api.Database.Schema.Template;

public record DbTableGroupTemplate
{
    public required int Id { get; init; }

    public required int TableTemplateId { get; init; }

    public required int? ParentTableGroupTemplateId { get; init; }

    public required string Title { get; init; }

    public required string? GroupKey { get; set; }

    public required string GroupType { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsDefault { get; init; }

    public required bool CanUserAddToTable { get; init; }

    public required bool ShowInContextMenu { get; init; }

    public required bool CanBeReordered { get; init; }

    public required bool CopyFromPrevious { get; init; }

    public required bool ShowContextMenu { get; init; }

    public virtual ICollection<DbTableRowTemplate> RowTemplates { get; init; } = [];

    public virtual ICollection<DbTableGroupTemplateWidget> TableGroupTemplateWidgets { get; init; } = [];
}
