using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Template;

namespace PUSpecSheet.Api.DAL.DataAccess.Template;

internal class TemplateDataAccessService : ITemplateDataAccessService
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly IUserContext _userContext;
    private readonly ILogger<ITemplateDataAccessService> _logger;

    public TemplateDataAccessService(PUSpecSheetDbContext dbContext,
        IUserContext userContext,
        ILogger<ITemplateDataAccessService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _userContext = userContext;
        _logger = logger;
    }

    #region Table Cell Types

    public async Task<IReadOnlyList<TableCellType>> GetAllTableCellTypesAsync(CancellationToken cancellationToken)
    {
        var tableCellTypes = await _dbContext.TableCellTypes
            .AsNoTracking()
            .MapToDomain()
            .ToListAsync(cancellationToken);
        if (tableCellTypes == null || tableCellTypes.Count == 0)
        {
            _logger.LogWarning("No TableCellTypes found in the database");
            return [];
        }

        _logger.LogInformation("Retrieved {TableCellTypeCount} TableCellTypes from the database", tableCellTypes.Count);
        return tableCellTypes;
    }

    #endregion Table Cell Types
}
