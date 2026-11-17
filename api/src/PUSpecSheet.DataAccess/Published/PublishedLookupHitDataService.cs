using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Published.Lookups;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Configurations.Values;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.DataAccess.Published;

public sealed class PublishedLookupHitDataService : IPublishedLookupHitDataService
{
    private readonly PuSpecSheetDbContext _db;

    public PublishedLookupHitDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<PublishedLookupHit>> ListAsync(PublishedLookupCriteria criteria, DateTime momentUtc, int? sheetId, CancellationToken cancellationToken)
    {
        return await Query(criteria, momentUtc, sheetId).ToListAsync(cancellationToken);
    }

    public IQueryable<PublishedLookupHit> Query(PublishedLookupCriteria criteria, DateTime momentUtc, int? sheetId)
    {
        var texts = Matching(_db.TextValues, TextValueConfiguration.LookupValue, criteria, momentUtc);
        var slots = Matching(_db.LabelledTextValues, LabelledTextValueConfiguration.LookupValue, criteria, momentUtc);

        // The cells first, so a labelled text cell with the value in two of its slots is still one hit.
        var cellIds = Narrow(texts, criteria, sheetId).Select(found => found.SheetCellId)
            .Concat(Narrow(slots, criteria, sheetId).Select(found => found.SheetCellId));
        return _db.SheetCells
            .AsNoTracking()
            .Where(cell => cellIds.Contains(cell.Id))
            .Select(cell => new PublishedLookupHit
            {
                SheetId = cell.SheetRow.SheetSection.SheetTable.SheetId,
                SheetPublicId = cell.SheetRow.SheetSection.SheetTable.Sheet.PublicId,
                PhaseCode = cell.SheetRow.SheetSection.SheetTable.Sheet.Phase.Code,
                SheetTypeId = cell.SheetRow.SheetSection.SheetTable.Sheet.SheetTypeId,
                TableId = cell.SheetRow.SheetSection.SheetTableId,
                SectionId = cell.SheetRow.SheetSectionId,
                RowId = cell.SheetRowId,
                CellId = cell.Id,
                ColumnBlockId = cell.SheetColumnBlockId,
                IsHeader = cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header,
            });
    }

    private static IQueryable<TValue> Narrow<TValue>(IQueryable<TValue> values, PublishedLookupCriteria criteria, int? sheetId)
        where TValue : class, IDbCellValue
    {
        if (sheetId is { } id)
        {
            values = values.Where(found => found.SheetRowRevision.SheetId == id);
        }

        if (criteria.PhaseCode is { } phaseCode)
        {
            values = values.Where(found => found.SheetCell.SheetRow.SheetSection.SheetTable.Sheet.Phase.Code == phaseCode);
        }

        if (criteria.SheetTypeId is { } sheetTypeId)
        {
            values = values.Where(found => found.SheetCell.SheetRow.SheetSection.SheetTable.Sheet.SheetTypeId == sheetTypeId);
        }

        return values;
    }

    // The index holds the start of each value; the whole value is compared too for the rare long one.
    private static IQueryable<TValue> Matching<TValue>(
        DbSet<TValue> set,
        string lookupColumn,
        PublishedLookupCriteria criteria,
        DateTime momentUtc)
        where TValue : class, IDbCellValue
    {
        var key = criteria.Key;
        var value = criteria.Value;
        var start = value.Length > TextValueConfiguration.LookupValueLength
            ? value[..TextValueConfiguration.LookupValueLength]
            : value;

        return set
            .AsNoTracking()
            .Where(found => EF.Property<string>(found, lookupColumn) == start
                && EF.Property<string>(found, nameof(DbTextValue.Value)) == value
                && found.SheetCell.TemplateCell.LookupKey == key
                && found.SheetRowRevision.Status == RevisionStatus.Published
                && found.SheetRowRevision.PublishedAtUtc <= momentUtc
                && (found.SheetRowRevision.SupersededAtUtc == null || found.SheetRowRevision.SupersededAtUtc > momentUtc)
                && !found.SheetRowRevision.IsDeleted);
    }
}
