using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Abstractions.Domain.Values;
using PUSpecSheet.Api.DAL.Query.Resolvers.Cells;
using PUSpecSheet.Query.Models.Table.Cell;
using PUSpecSheet.Query.Models.Values;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class CellMapper
{
    public static async Task<SlimPUSpecSheetCell?> MapToSlimCell(this SpecSheetTableCell cell,
        SpecSheet specSheet,
        LinkedSpecSheetLookupCellResolver linkedSpecSheetLookupCellResolver,
        CancellationToken cancellationToken)
    {
        return cell.CellValue switch
        {
            LinkedSpecSheetValue linkedSpecSheetValue => await linkedSpecSheetLookupCellResolver.ResolveAsync(specSheet, linkedSpecSheetValue, cancellationToken),
            NumericValue numericValue => new SlimPUSpecSheetCell
            {
                Id = cell.Id,
                ReferenceId = cell.ReferenceId,
                ColumnNumber = cell.ColumnNumber,
                RowSpan = cell.RowSpan,
                ColSpan = cell.ColSpan,
                CellType = cell.TableCellType!.MapToSlimType(),
                IsIdentifier = cell.CellConfiguration!.IsIdentifier,
                Value = new SlimDoubleValue
                {
                    Id = cell.CellValue.Id,
                    Value = numericValue.Value
                }
            },
            TextValue textValue => new SlimPUSpecSheetCell
            {
                Id = cell.Id,
                ReferenceId = cell.ReferenceId,
                ColumnNumber = cell.ColumnNumber,
                RowSpan = cell.RowSpan,
                ColSpan = cell.ColSpan,
                CellType = cell.TableCellType!.MapToSlimType(),
                IsIdentifier = cell.CellConfiguration!.IsIdentifier,
                Value = new SlimTextValue
                {
                    Id = cell.CellValue.Id,
                    Value = textValue.Value
                }
            },
            PairValue pairValue => new SlimPUSpecSheetCell
            {
                Id = cell.Id,
                ReferenceId = cell.ReferenceId,
                ColumnNumber = cell.ColumnNumber,
                RowSpan = cell.RowSpan,
                ColSpan = cell.ColSpan,
                CellType = cell.TableCellType!.MapToSlimType(),
                IsIdentifier = cell.CellConfiguration!.IsIdentifier,
                Value = new SlimPairValue
                {
                    Id = cell.CellValue.Id,
                    ValueX = pairValue.ValueX,
                    ValueY = pairValue.ValueY
                }
            },
            _ => throw new NotImplementedException(),
        };
    }
}
