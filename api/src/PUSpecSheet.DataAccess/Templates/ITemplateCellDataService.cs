using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public interface ITemplateCellDataService
{
    Task<DbTemplateCell?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<DbTemplateCell?> FindWithTypeForEditAsync(int id, CancellationToken cancellationToken);

    Task<bool> ColumnTakenAsync(int rowId, int? columnBlockId, int column, int exceptId, CancellationToken cancellationToken);

    Task ShiftRightAsync(int rowId, int? columnBlockId, int fromColumn, CancellationToken cancellationToken);

    Task DeleteInColumnBlockAsync(int columnBlockId, CancellationToken cancellationToken);

    Task<List<DbTemplateCell>> ListAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    Task<bool> LookupKeyExistsAsync(string key, CancellationToken cancellationToken);
}
