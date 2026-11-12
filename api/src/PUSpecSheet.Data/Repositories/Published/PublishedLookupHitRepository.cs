using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Published;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Data.Configurations.Values;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Repositories.Published;

/// <summary>
/// Finds the cells that held a value at a moment, in one query. It seeks the value in the text value
/// indexes (text cells, and each value of a labelled text cell), then keeps the values whose cell has the
/// lookup key and whose row revision was the published one at that moment, so the cost follows how many
/// cells hold the value and not how much data there is.
/// </summary>
public sealed class PublishedLookupHitRepository(PuSpecSheetDbContext db) : IPublishedLookupHitRepository
{
    /// <param name="criteria">The key and value to find, and the phase or sheet type to keep to.</param>
    /// <param name="momentUtc">The moment the sheets are read at.</param>
    /// <param name="sheetId">The one sheet to search, or null for every sheet.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    public async Task<List<PublishedLookupHit>> ListAsync(PublishedLookupCriteria criteria, DateTime momentUtc, int? sheetId, CancellationToken cancellationToken)
    {
        return await Query(criteria, momentUtc, sheetId).ToListAsync(cancellationToken);
    }

    public IQueryable<PublishedLookupHit> Query(PublishedLookupCriteria criteria, DateTime momentUtc, int? sheetId)
    {
        var texts = Matching(db.TextValues, TextValueConfiguration.LookupValue, criteria, momentUtc);
        var slots = Matching(db.LabelledTextValues, LabelledTextValueConfiguration.LookupValue, criteria, momentUtc);

        // The cells first, so a labelled text cell with the value in two of its slots is still one hit.
        var cellIds = Narrow(texts, criteria, sheetId).Select(found => found.SheetCellId)
            .Concat(Narrow(slots, criteria, sheetId).Select(found => found.SheetCellId));
        return db.SheetCells
            .AsNoTracking()
            .Where(cell => cellIds.Contains(cell.Id))
            .Select(cell => new PublishedLookupHit(
                cell.SheetRow.SheetSection.SheetTable.SheetId,
                cell.SheetRow.SheetSection.SheetTable.Sheet.PublicId,
                cell.SheetRow.SheetSection.SheetTable.Sheet.Phase.Code,
                cell.SheetRow.SheetSection.SheetTable.Sheet.SheetTypeId,
                cell.SheetRow.SheetSection.SheetTableId,
                cell.SheetRow.SheetSectionId,
                cell.SheetRowId,
                cell.Id,
                cell.SheetColumnBlockId,
                cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header));
    }

    private static IQueryable<TValue> Narrow<TValue>(IQueryable<TValue> values, PublishedLookupCriteria criteria, int? sheetId)
        where TValue : class, ICellValue
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

    /// <summary>
    /// The published values of one table under the key that equal the value at the moment. The index holds
    /// the start of each value; the whole value is compared too for the rare long one.
    /// </summary>
    private static IQueryable<TValue> Matching<TValue>(
        DbSet<TValue> set,
        string lookupColumn,
        PublishedLookupCriteria criteria,
        DateTime momentUtc)
        where TValue : class, ICellValue
    {
        var key = criteria.Key;
        var value = criteria.Value;
        var start = value.Length > TextValueConfiguration.LookupValueLength
            ? value[..TextValueConfiguration.LookupValueLength]
            : value;

        return set
            .AsNoTracking()
            .Where(found => EF.Property<string>(found, lookupColumn) == start
                && EF.Property<string>(found, nameof(TextValue.Value)) == value
                && found.SheetCell.TemplateCell.LookupKey == key
                && found.SheetRowRevision.Status == RevisionStatus.Published
                && found.SheetRowRevision.PublishedAtUtc <= momentUtc
                && (found.SheetRowRevision.SupersededAtUtc == null || found.SheetRowRevision.SupersededAtUtc > momentUtc)
                && !found.SheetRowRevision.IsDeleted);
    }
}
