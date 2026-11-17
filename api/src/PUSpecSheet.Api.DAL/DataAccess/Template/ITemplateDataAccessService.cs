using PUSpecSheet.Abstractions.Domain.Template;

namespace PUSpecSheet.Api.DAL.DataAccess.Template;

public interface ITemplateDataAccessService
{
    #region Table Cell Types

    Task<IReadOnlyList<TableCellType>> GetAllTableCellTypesAsync(CancellationToken cancellationToken);

    #endregion Table Cell Types
}
