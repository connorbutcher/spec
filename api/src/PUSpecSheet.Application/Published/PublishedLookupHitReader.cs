using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Data.Configurations.Values;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Finds the cells that held a value at a moment, in one query. It seeks the value in the text value
/// index, then keeps the values whose cell has the lookup key and whose row revision was the published one
/// at that moment, so the cost follows how many cells hold the value and not how much data there is.
/// </summary>
public sealed class PublishedLookupHitReader(PuSpecSheetDbContext db)
{
    /// <param name="criteria">The key and value to find, and the phase or sheet type to keep to.</param>
    /// <param name="momentUtc">The moment the sheets are read at.</param>
    /// <param name="sheetId">The one sheet to search, or null for every sheet.</param>
    public IQueryable<PublishedLookupHit> Query(PublishedLookupCriteria criteria, DateTime momentUtc, int? sheetId)
    {
        var key = criteria.Key;
        var value = criteria.Value;

        // The index holds the start of each value; the whole value is compared too for the rare long one.
        var start = value.Length > TextValueConfiguration.LookupValueLength
            ? value[..TextValueConfiguration.LookupValueLength]
            : value;

        var values = db.TextValues
            .AsNoTracking()
            .Where(text => EF.Property<string>(text, TextValueConfiguration.LookupValue) == start
                && text.Value == value
                && text.SheetCell.TemplateCell.LookupKey == key
                && text.SheetRowRevision.Status == RevisionStatus.Published
                && text.SheetRowRevision.PublishedAtUtc <= momentUtc
                && (text.SheetRowRevision.SupersededAtUtc == null || text.SheetRowRevision.SupersededAtUtc > momentUtc)
                && !text.SheetRowRevision.IsDeleted);

        if (sheetId is { } id)
        {
            values = values.Where(text => text.SheetCell.SheetRow.SheetSection.SheetTable.SheetId == id);
        }

        if (criteria.PhaseCode is { } phaseCode)
        {
            values = values.Where(text => text.SheetCell.SheetRow.SheetSection.SheetTable.Sheet.Phase.Code == phaseCode);
        }

        if (criteria.SheetTypeId is { } sheetTypeId)
        {
            values = values.Where(text => text.SheetCell.SheetRow.SheetSection.SheetTable.Sheet.SheetTypeId == sheetTypeId);
        }

        return values.Select(text => new PublishedLookupHit(
            text.SheetCell.SheetRow.SheetSection.SheetTable.SheetId,
            text.SheetCell.SheetRow.SheetSection.SheetTable.Sheet.PublicId,
            text.SheetCell.SheetRow.SheetSection.SheetTable.Sheet.Phase.Code,
            text.SheetCell.SheetRow.SheetSection.SheetTable.Sheet.SheetTypeId,
            text.SheetCell.SheetRow.SheetSection.SheetTableId,
            text.SheetCell.SheetRow.SheetSectionId,
            text.SheetCell.SheetRowId,
            text.SheetCellId,
            text.SheetCell.SheetColumnBlockId,
            text.SheetCell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header));
    }
}
