using System.Text.Json.Serialization;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Abstractions.Operations.TableGroups;
using PUSpecSheet.Abstractions.Operations.TableGroups.Widgets;
using PUSpecSheet.Abstractions.Operations.Tables;
using PUSpecSheet.Abstractions.Operations.Tables.Notes;
using PUSpecSheet.Abstractions.Operations.Tables.Widgets;
using PUSpecSheet.Abstractions.Operations.Values.Linked;
using PUSpecSheet.Abstractions.Operations.Values.Number;
using PUSpecSheet.Abstractions.Operations.Values.Pair;
using PUSpecSheet.Abstractions.Operations.Values.Text;

namespace PUSpecSheet.Abstractions.Operations;

/// <summary>
/// An operation to change data.
/// </summary>
[JsonDerivedType(typeof(AddSpecSheetTableOp), "AddSpecSheetTableOp")]
[JsonDerivedType(typeof(UpdateSpecSheetTableOp), "UpdateSpecSheetTableOp")]
[JsonDerivedType(typeof(DeleteSpecSheetTableOp), "DeleteSpecSheetTableOp")]
[JsonDerivedType(typeof(AddSpecSheetTableWidgetOp), "AddSpecSheetTableWidgetOp")]
[JsonDerivedType(typeof(UpdateSpecSheetTableWidgetOp), "UpdateSpecSheetTableWidgetOp")]
[JsonDerivedType(typeof(AddSpecSheetTableNoteOp), "AddSpecSheetTableNoteOp")]
[JsonDerivedType(typeof(UpdateSpecSheetTableNoteOp), "UpdateSpecSheetTableNoteOp")]

[JsonDerivedType(typeof(AddSpecSheetTableGroupOp), "AddSpecSheetTableGroupOp")]
[JsonDerivedType(typeof(UpdateSpecSheetTableGroupOp), "UpdateSpecSheetTableGroupOp")]
[JsonDerivedType(typeof(DeleteSpecSheetTableGroupOp), "DeleteSpecSheetTableGroupOp")]
[JsonDerivedType(typeof(AddSpecSheetTableGroupWidgetOp), "AddSpecSheetTableGroupWidgetOp")]
[JsonDerivedType(typeof(UpdateSpecSheetTableGroupWidgetOp), "UpdateSpecSheetTableGroupWidgetOp")]

[JsonDerivedType(typeof(AddSpecSheetTableRowOp), "AddSpecSheetTableRowOp")]
[JsonDerivedType(typeof(UpdateSpecSheetTableRowOp), "UpdateSpecSheetTableRowOp")]
[JsonDerivedType(typeof(DeleteSpecSheetTableRowOp), "DeleteSpecSheetTableRowOp")]

[JsonDerivedType(typeof(AddSpecSheetTableCellOp), "AddSpecSheetTableCellOp")]
[JsonDerivedType(typeof(UpdateSpecSheetTableCellOp), "UpdateSpecSheetTableCellOp")]
[JsonDerivedType(typeof(DeleteSpecSheetTableCellOp), "DeleteSpecSheetTableCellOp")]

[JsonDerivedType(typeof(AddTextValueOp), "AddTextValueOp")]
[JsonDerivedType(typeof(UpdateTextValueOp), "UpdateTextValueOp")]

[JsonDerivedType(typeof(AddNumberValueOp), "AddNumberValueOp")]
[JsonDerivedType(typeof(UpdateNumberValueOp), "UpdateNumberValueOp")]

[JsonDerivedType(typeof(AddPairValueOp), "AddPairValueOp")]
[JsonDerivedType(typeof(UpdatePairValueOp), "UpdatePairValueOp")]

[JsonDerivedType(typeof(AddLinkedSpecSheetValueOp), "AddLinkedSpecSheetValueOp")]
[JsonDerivedType(typeof(UpdateLinkedSpecSheetValueOp), "UpdateLinkedSpecSheetValueOp")]
public interface IChangeOp
{
}
