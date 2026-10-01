import { CellKind } from '../templates/models/cell-kind';
import { CellValueRequest } from './models/cell-value-request.model';
import { SheetCell } from './models/sheet-cell.model';

/** A date as `yyyy-MM-dd` from its local parts, so the day doesn't shift with the time zone. */
export function toDateValue(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

/** A local date from a `yyyy-MM-dd` value, or null for none. */
export function fromDateValue(value: string | null): Date | null {
  if (!value) {
    return null;
  }
  const [year, month, day] = value.split('-').map(Number);
  return new Date(year, month - 1, day);
}

/** A request that sets one cell's value, in the field the cell's kind uses. A null value clears the cell. */
export function valueRequest(
  cell: SheetCell,
  kind: CellKind,
  value: string | number | boolean | Date | null,
): CellValueRequest {
  const request: CellValueRequest = { sheetCellId: cell.id };
  switch (kind) {
    case 'Text': {
      request.text = typeof value === 'string' ? value : null;
      break;
    }
    case 'Number': {
      request.number = typeof value === 'number' ? value : null;
      break;
    }
    case 'Date': {
      request.date = value instanceof Date ? toDateValue(value) : null;
      break;
    }
    case 'Checkbox': {
      request.boolean = typeof value === 'boolean' ? value : null;
      break;
    }
    case 'TextDropdown':
    case 'NumberDropdown': {
      request.optionId = typeof value === 'number' ? value : null;
      break;
    }
  }
  return request;
}
