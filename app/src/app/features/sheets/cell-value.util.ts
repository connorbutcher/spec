import { CellConfiguration } from '../templates/models/cell-configuration';
import { CellKind } from '../templates/models/cell-kind';
import { CellType } from '../templates/models/cell-type.model';
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

/** What a value cell shows as plain text when it isn't a control. Empty when it has no value, and for a checkbox. */
export function displayValue(
  cell: SheetCell,
  cellType: CellType,
  configuration: CellConfiguration,
): string {
  switch (cellType.kind) {
    case 'Text': {
      return cell.textValue ?? '';
    }
    case 'Number': {
      return configuration.kind === 'Number'
        ? formatNumber(cell.numberValue, configuration.decimalPlaces, configuration.unit)
        : formatNumber(cell.numberValue, null, null);
    }
    case 'Date': {
      return cell.dateValue ?? '';
    }
    case 'TextDropdown':
    case 'NumberDropdown': {
      return cellType.options.find((option) => option.id === cell.optionId)?.value ?? '';
    }
    default: {
      return '';
    }
  }
}

/** A number to its cell's decimal places, followed by its unit: "12.50 Nm". */
function formatNumber(
  value: number | null,
  decimalPlaces: number | null | undefined,
  unit: string | null | undefined,
): string {
  if (value === null) {
    return '';
  }
  const text =
    decimalPlaces === null || decimalPlaces === undefined
      ? String(value)
      : value.toFixed(decimalPlaces);
  return unit ? `${text} ${unit}` : text;
}
