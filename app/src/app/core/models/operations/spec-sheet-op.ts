import { AddSpecSheetTableCellOp } from './cells/add-spec-sheet-table-cell-op';
import { DeleteSpecSheetTableCellOp } from './cells/delete-spec-sheet-table-cell-op';
import { UpdateSpecSheetTableCellOp } from './cells/update-spec-sheet-table-cell-op';
import { AddSpecSheetTableGroupOp } from './rows/add-spec-sheet-table-row-op';
import { DeleteSpecSheetTableRowOp } from './rows/delete-spec-sheet-table-row-op';
import { UpdateSpecSheetTableRowOp } from './rows/update-spec-sheet-table-row-op';
import { AddSpecSheetTableRowOp } from './table-groups/add-spec-sheet-table-group-op';
import { DeleteSpecSheetTableGroupOp } from './table-groups/delete-spec-sheet-table-group-op';
import { UpdateSpecSheetTableGroupOp } from './table-groups/update-spec-sheet-table-group-op';
import { AddSpecSheetTableGroupWidgetOp } from './table-groups/widgets/add-spec-sheet-table-widget-group-op';
import { UpdateSpecSheetTableGroupWidgetOp } from './table-groups/widgets/update-spec-sheet-table-widget-group-op';
import { AddSpecSheetTableOp } from './tables/add-spec-sheet-table-op';
import { DeleteSpecSheetTableOp } from './tables/delete-spec-sheet-table-op';
import { AddSpecSheetTableNoteOp } from './tables/notes/add-spec-sheet-table-note-op';
import { UpdateSpecSheetTableNoteOp } from './tables/notes/update-spec-sheet-table-note-op';
import { UpdateSpecSheetTableOp } from './tables/update-spec-sheet-table-op';
import { AddSpecSheetTableWidgetOp } from './tables/widgets/add-spec-sheet-table-widget-op';
import { UpdateSpecSheetTableWidgetOp } from './tables/widgets/update-spec-sheet-table-widget-op';
import { AddLinkedSpecSheetValueOp } from './values/linked/add-linked-spec-sheet-value-op';
import { UpdateLinkedSpecSheetValueOp } from './values/linked/update-linked-spec-sheet-value-op';
import { AddNumberValueOp } from './values/number/add-number-value-op';
import { UpdateNumberValueOp } from './values/number/update-number-value-op';
import { AddPairValueOp } from './values/pair/add-pair-value-op';
import { UpdatePairValueOp } from './values/pair/update-number-value-op';
import { AddTextValueOp } from './values/text/add-text-value-op';
import { UpdateTextValueOp } from './values/text/update-text-value-op';

export type SpecSheetOp =
  | AddSpecSheetTableOp
  | UpdateSpecSheetTableOp
  | DeleteSpecSheetTableOp
  | AddSpecSheetTableWidgetOp
  | UpdateSpecSheetTableWidgetOp
  | AddSpecSheetTableNoteOp
  | UpdateSpecSheetTableNoteOp
  | AddSpecSheetTableGroupOp
  | UpdateSpecSheetTableGroupOp
  | DeleteSpecSheetTableGroupOp
  | AddSpecSheetTableGroupWidgetOp
  | UpdateSpecSheetTableGroupWidgetOp
  | AddSpecSheetTableRowOp
  | UpdateSpecSheetTableRowOp
  | DeleteSpecSheetTableRowOp
  | AddSpecSheetTableCellOp
  | UpdateSpecSheetTableCellOp
  | DeleteSpecSheetTableCellOp
  | AddTextValueOp
  | UpdateTextValueOp
  | AddNumberValueOp
  | UpdateNumberValueOp
  | AddPairValueOp
  | UpdatePairValueOp
  | AddLinkedSpecSheetValueOp
  | UpdateLinkedSpecSheetValueOp;
