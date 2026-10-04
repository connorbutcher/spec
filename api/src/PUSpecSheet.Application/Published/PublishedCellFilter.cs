using System.Linq.Expressions;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Builds the "any of these" filter for the cell query from only the kinds of item the caller named, so a
/// request for a few cells is a seek on their identifiers and not a scan of the sheet.
/// </summary>
internal static class PublishedCellFilter
{
    public static Expression<Func<SheetCell, bool>>? For(PublishedSelectionScope scope)
    {
        var terms = new List<Expression<Func<SheetCell, bool>>>();

        if (scope.TableIds.Count > 0)
        {
            var tableIds = scope.TableIds;
            terms.Add(cell => tableIds.Contains(cell.SheetRow.SheetSection.SheetTableId));
        }

        if (scope.SectionIds.Count > 0)
        {
            var sectionIds = scope.SectionIds;
            terms.Add(cell => sectionIds.Contains(cell.SheetRow.SheetSectionId));
        }

        if (scope.RowPublicIds.Count > 0)
        {
            var rowIds = scope.RowPublicIds;
            terms.Add(cell => rowIds.Contains(cell.SheetRow.PublicId));
        }

        if (scope.CellPublicIds.Count > 0)
        {
            var cellIds = scope.CellPublicIds;
            terms.Add(cell => cellIds.Contains(cell.PublicId));
        }

        if (terms.Count == 0)
        {
            return null;
        }

        var parameter = terms[0].Parameters[0];
        var body = terms[0].Body;
        foreach (var term in terms.Skip(1))
        {
            body = Expression.OrElse(body, new ParameterReplacer(term.Parameters[0], parameter).Visit(term.Body));
        }

        return Expression.Lambda<Func<SheetCell, bool>>(body, parameter);
    }
}
