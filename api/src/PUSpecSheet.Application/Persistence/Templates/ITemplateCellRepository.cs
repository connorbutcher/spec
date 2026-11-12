using PUSpecSheet.Database.Entities.Templates;

namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>The cells of template rows.</summary>
public interface ITemplateCellRepository
{
    /// <summary>The cell with its row and the row's section, tracked for editing; null when there is no such cell.</summary>
    Task<TemplateCell?> FindForEditAsync(int id, CancellationToken cancellationToken);

    /// <summary>The cell with its cell type, row and the row's section, tracked for editing; null when there is no such cell.</summary>
    Task<TemplateCell?> FindWithTypeForEditAsync(int id, CancellationToken cancellationToken);

    /// <summary>Whether a cell other than <paramref name="exceptId"/> starts at the column of the row (and column block).</summary>
    Task<bool> ColumnTakenAsync(int rowId, int? columnBlockId, int column, int exceptId, CancellationToken cancellationToken);

    /// <summary>
    /// Moves the row's cells (in the column block, if any) from the column on one to the right, in one
    /// statement so the one-cell-per-column index is only checked once they've all moved.
    /// </summary>
    Task ShiftRightAsync(int rowId, int? columnBlockId, int fromColumn, CancellationToken cancellationToken);

    /// <summary>Deletes the column block's cells straight in the database.</summary>
    Task DeleteInColumnBlockAsync(int columnBlockId, CancellationToken cancellationToken);

    /// <summary>The cells asked for, read only.</summary>
    Task<List<TemplateCell>> ListAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    /// <summary>Whether any template cell has the lookup key.</summary>
    Task<bool> LookupKeyExistsAsync(string key, CancellationToken cancellationToken);

    void Add(TemplateCell cell);

    void Remove(TemplateCell cell);
}
