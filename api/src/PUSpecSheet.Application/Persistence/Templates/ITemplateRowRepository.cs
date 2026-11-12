using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>The rows of template sections.</summary>
public interface ITemplateRowRepository
{
    /// <summary>Every row of a version with its cells, read only.</summary>
    Task<List<TemplateRow>> ListWithCellsAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>The ids of every row of a version.</summary>
    Task<List<int>> ListIdsAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>A section's rows with their cells, in display order, tracked for editing.</summary>
    Task<List<TemplateRow>> ListInSectionWithCellsForEditAsync(int sectionId, CancellationToken cancellationToken);

    /// <summary>A section's rows, tracked for editing.</summary>
    Task<List<TemplateRow>> ListInSectionForEditAsync(int sectionId, CancellationToken cancellationToken);

    /// <summary>The row with its section, tracked for editing; null when there is no such row.</summary>
    Task<TemplateRow?> FindForEditAsync(int id, CancellationToken cancellationToken);

    /// <summary>Where a new cell in the row (and column block, if any) goes; null when there is no such row.</summary>
    Task<TemplateCellTarget?> FindCellTargetAsync(int rowId, int? columnBlockId, CancellationToken cancellationToken);

    void Add(TemplateRow row);

    void Remove(TemplateRow row);
}
