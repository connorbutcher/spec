using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// The value of a cell in the revision of its row that is shown, of the kind its cell type holds.
/// </summary>
[JsonDerivedType(typeof(SheetTextValue), typeDiscriminator: "text")]
[JsonDerivedType(typeof(SheetNumericValue), typeDiscriminator: "numeric")]
[JsonDerivedType(typeof(SheetPairValue), typeDiscriminator: "pair")]
[JsonDerivedType(typeof(SheetLinkedSheetValue), typeDiscriminator: "linkedSpecSheet")]
public abstract record SheetCellValue
{
    public required int SheetTableCellId { get; init; }
}
