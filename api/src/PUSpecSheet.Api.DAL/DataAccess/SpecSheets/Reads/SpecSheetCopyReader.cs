using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.Legacy.SpecSheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// Reads a spec sheet from its copy: the SpecSheets row and the tables, groups, rows, cells and values under it.
/// </summary>
internal class SpecSheetCopyReader
{
    private readonly PUSpecSheetDbContext _dbContext;

    public SpecSheetCopyReader(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    public async Task<SpecSheet> ReadAsync(int id, CancellationToken cancellationToken)
    {
        var specSheet = await _dbContext.SpecSheets
        .Where(x => x.Id == id)
        .Include(x => x.CreatedByUser)
        .Include(x => x.PublishedByUser)
        .Include(x => x.SpecSheetType)
        .Include(x => x.SpecSheetPhase)

        .Include(x => x.PropertyValues)
            .ThenInclude(x => x.SpecSheetTypeProperty)

        .Include(x => x.WorkflowSteps)
            .ThenInclude(x => x.Actors)
                .ThenInclude(x => x.ActorUser)

        .Include(x => x.ExternalReferences)

        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Widgets)
                .ThenInclude(x => x.Value)
        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Widgets)
                .ThenInclude(x => x.TableTemplateWidget)
        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Notes)
                .ThenInclude(x => x.CreatedByUser)

        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                .ThenInclude(x => x.Widgets)
                    .ThenInclude(x => x.Value)
        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                .ThenInclude(x => x.Widgets)
                    .ThenInclude(x => x.TableGroupTemplateWidget)

        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                .ThenInclude(x => x.Rows.Where(r => !r.IsDeleted).OrderBy(r => r.RowNumber))
                    .ThenInclude(x => x.Cells.Where(c => !c.IsDeleted).OrderBy(c => c.RowNumber).ThenBy(c => c.ColumnNumber))
                        .ThenInclude(x => x.CellConfiguration)

        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                .ThenInclude(x => x.Rows.Where(r => !r.IsDeleted).OrderBy(r => r.RowNumber))
                    .ThenInclude(x => x.Cells.Where(c => !c.IsDeleted).OrderBy(c => c.RowNumber).ThenBy(c => c.ColumnNumber))
                        .ThenInclude(x => x.TableCellType)

        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                .ThenInclude(x => x.Rows.Where(r => !r.IsDeleted).OrderBy(r => r.RowNumber))
                    .ThenInclude(x => x.Cells.Where(c => !c.IsDeleted).OrderBy(c => c.RowNumber).ThenBy(c => c.ColumnNumber))
                        .ThenInclude(x => x.TextValue)
        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                .ThenInclude(x => x.Rows.Where(r => !r.IsDeleted).OrderBy(r => r.RowNumber))
                    .ThenInclude(x => x.Cells.Where(c => !c.IsDeleted).OrderBy(c => c.RowNumber).ThenBy(c => c.ColumnNumber))
                        .ThenInclude(x => x.NumericValue)
        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                .ThenInclude(x => x.Rows.Where(r => !r.IsDeleted).OrderBy(r => r.RowNumber))
                    .ThenInclude(x => x.Cells.Where(c => !c.IsDeleted).OrderBy(c => c.RowNumber).ThenBy(c => c.ColumnNumber))
                        .ThenInclude(x => x.PairValue)
        .Include(x => x.Tables.Where(t => !t.IsDeleted).OrderBy(t => t.DisplayOrder))
            .ThenInclude(x => x.Groups.Where(g => !g.IsDeleted).OrderBy(g => g.Order))
                .ThenInclude(x => x.Rows.Where(r => !r.IsDeleted).OrderBy(r => r.RowNumber))
                    .ThenInclude(x => x.Cells.Where(c => !c.IsDeleted).OrderBy(c => c.RowNumber).ThenBy(c => c.ColumnNumber))
                        .ThenInclude(x => x.LinkedSpecSheetValue)

        .AsSplitQuery()
        .AsNoTracking()
        .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException($"Spec Sheet with Id {id} not found.");

        return specSheet.MapToDomain();
    }
}
