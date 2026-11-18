using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.DAL.Query.Resolvers.Cells;
using PUSpecSheet.Query.Models.Table.Cell;
using PUSpecSheet.Query.Models.Values;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class CellMapper
{
    public static async Task<SlimPUSpecSheetCell?> MapToSlimCell(this SheetTableCell cell,
        Sheet sheet,
        LinkedSpecSheetLookupCellResolver linkedSpecSheetLookupCellResolver,
        CancellationToken cancellationToken)
    {
        return cell.Value switch
        {
            SheetLinkedSheetValue linkedSheetValue => await linkedSpecSheetLookupCellResolver.ResolveAsync(sheet, linkedSheetValue, cancellationToken),
            SheetNumericValue numericValue => cell.MapToSlimCell(new SlimDoubleValue
            {
                Id = numericValue.SheetTableCellId,
                Value = numericValue.Value
            }),
            SheetTextValue textValue => cell.MapToSlimCell(new SlimTextValue
            {
                Id = textValue.SheetTableCellId,
                Value = textValue.Value
            }),
            SheetPairValue pairValue => cell.MapToSlimCell(new SlimPairValue
            {
                Id = pairValue.SheetTableCellId,
                ValueX = pairValue.ValueX,
                ValueY = pairValue.ValueY
            }),
            _ => throw new NotImplementedException(),
        };
    }

    private static SlimPUSpecSheetCell MapToSlimCell(this SheetTableCell cell, SlimValue value)
    {
        return new SlimPUSpecSheetCell
        {
            Id = cell.Id,
            ReferenceId = cell.ReferenceId,
            ColumnNumber = cell.ColumnNumber,
            RowSpan = cell.RowSpan,
            ColSpan = cell.ColSpan,
            CellType = cell.TableCellType!.MapToSlimType(),
            IsIdentifier = cell.CellConfiguration!.IsIdentifier,
            Value = value
        };
    }
}
