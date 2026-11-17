using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published;

public interface IPublishedStructureDataService
{
    Task<PublishedStructure> ReadAsync(ResolvedSheetVersion version, CancellationToken cancellationToken);

    IQueryable<PublishedTableRecord> Tables(ResolvedSheetVersion version);

    IQueryable<PublishedSectionRecord> Sections(ResolvedSheetVersion version);

    IQueryable<PublishedColumnBlockRecord> ColumnBlocks(ResolvedSheetVersion version);
}
