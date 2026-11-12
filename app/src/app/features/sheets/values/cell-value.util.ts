import { CellConfiguration } from '../../templates/models/cell-configuration';
import { CellKind } from '../../templates/models/cell-kind';
import { CellType } from '../../templates/models/cell-type.model';
import { CellValue } from '../models/cell-value';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';

export class CellValueUtil {
  /** A date as `yyyy-MM-dd` from its local parts, so the day doesn't shift with the time zone. */
  public static toDateValue(date: Date): string {
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${date.getFullYear()}-${month}-${day}`;
  }

  /** A local date from a `yyyy-MM-dd` value, or null for none. */
  public static fromDateValue(value: string | null): Date | null {
    if (!value) {
      return null;
    }
    const [year, month, day] = value.split('-').map(Number);
    return new Date(year, month - 1, day);
  }

  /** A text or linked dropdown cell's text, or null when it holds none. */
  public static text(cell: SheetCell | null | undefined): string | null {
    return typeof cell?.value === 'string' ? cell.value : null;
  }

  /** A number cell's number, or null when it holds none. */
  public static number(cell: SheetCell | null | undefined): number | null {
    return typeof cell?.value === 'number' ? cell.value : null;
  }

  /** A date cell's date as `yyyy-MM-dd`, or null when it holds none. */
  public static date(cell: SheetCell | null | undefined): string | null {
    return typeof cell?.value === 'string' ? cell.value : null;
  }

  /** A checkbox's state, or null when it has never been set. */
  public static boolean(cell: SheetCell | null | undefined): boolean | null {
    return typeof cell?.value === 'boolean' ? cell.value : null;
  }

  /** The id of a dropdown cell's chosen option, or null when nothing is chosen. */
  public static optionId(cell: SheetCell | null | undefined): number | null {
    return typeof cell?.value === 'number' ? cell.value : null;
  }

  /** A labelled text cell's values by slot key, such as `{ rh: 'P-1003-R' }`, or null when it holds none. */
  public static slots(cell: SheetCell | null | undefined): Readonly<Record<string, string>> | null {
    const value = cell?.value;
    return typeof value === 'object' && value !== null ? value : null;
  }

  /** A request that sets one cell's value, in the shape the cell's kind takes. A null value clears the cell. */
  public static valueRequest(cell: SheetCell, kind: CellKind, value: string | number | boolean | Date | null): CellValueRequest {
    return { sheetCellId: cell.id, value: CellValueUtil.requestValue(kind, value) };
  }

  /** What a value cell shows as plain text when it isn't a control. Empty when it has no value, and for a checkbox. */
  public static displayValue(cell: SheetCell, cellType: CellType, configuration: CellConfiguration): string {
    switch (cellType.kind) {
      case 'Text':
      case 'LinkedDropdown': {
        return CellValueUtil.text(cell) ?? '';

      }
      case 'Number': {
        return configuration.kind === 'Number'
          ? CellValueUtil.formatNumber(CellValueUtil.number(cell), configuration.decimalPlaces, configuration.unit)
          : CellValueUtil.formatNumber(CellValueUtil.number(cell), null, null);
      }
      case 'Date': {
        return CellValueUtil.date(cell) ?? '';
      }
      case 'TextDropdown':
      case 'NumberDropdown': {
        return cellType.options.find((option) => option.id === CellValueUtil.optionId(cell))?.value ?? '';
      }
      case 'LabelledText': {
        return Object.values(CellValueUtil.slots(cell) ?? {}).join(' / ');
      }
      default: {
        return '';
      }
    }
  }

  /** A number to its cell's decimal places, followed by its unit: "12.50 Nm". */
  private static formatNumber(value: number | null, decimalPlaces: number | null | undefined, unit: string | null | undefined): string {
    if (value === null) {
      return '';
    }
    const text =
      decimalPlaces === null || decimalPlaces === undefined
        ? String(value)
        : value.toFixed(decimalPlaces);
    return unit ? `${text} ${unit}` : text;
  }

  /** A value in the shape the cell's kind takes, or null when it doesn't fit the kind. */
  private static requestValue(kind: CellKind, value: string | number | boolean | Date | null): CellValue {
    switch (kind) {
      case 'Text':
      case 'LinkedDropdown': {
        return typeof value === 'string' ? value : null;
      }
      case 'Number':
      case 'TextDropdown':
      case 'NumberDropdown': {
        return typeof value === 'number' ? value : null;
      }
      case 'Date': {
        return value instanceof Date ? CellValueUtil.toDateValue(value) : null;
      }
      case 'Checkbox': {
        return typeof value === 'boolean' ? value : null;
      }
      default: {
        return null;
      }
    }
  }
}
