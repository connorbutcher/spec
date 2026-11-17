using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Domain.Properties.Cell;

[JsonDerivedType(typeof(NumberCellProperties), typeDiscriminator: "numeric")]
[JsonDerivedType(typeof(TextCellProperties), typeDiscriminator: "text")]
[JsonDerivedType(typeof(PairCellProperties), typeDiscriminator: "pair")]
[JsonDerivedType(typeof(NumericDropdownCellProperties), typeDiscriminator: "numericDropdown")]
[JsonDerivedType(typeof(TextDropdownCellProperties), typeDiscriminator: "textDropdown")]
[JsonDerivedType(typeof(LinkedSpecSheetLookupCellProperties), typeDiscriminator: "linkedSpecSheet")]
public interface ICellProperties
{
}
