import { CellType } from '../../templates/models/cell-type.model';
import { CellValueUtil } from './cell-value.util';
import { SheetCell } from '../models/sheet-cell.model';

class SpecHelpers {
  public static cell(values: Partial<SheetCell> = {}): SheetCell {
    return {
      id: 7,
      publicId: 'c1',
      template: {
        id: 1,
        cellTypeId: 1,
        column: 1,
        rowSpan: 1,
        columnSpan: 1,
        caption: null,
        isRequired: false,
        configurationOverride: null,
        styleOverride: null,
        columnBlockId: null,
      },
      value: null,
      settings: null,
      sheetColumnBlockId: null,
      lastChange: null,
      ...values,
    };
  }

  public static cellType(values: Partial<CellType> = {}): CellType {
    return {
      id: 1,
      name: 'Type',
      kind: 'Text',
      description: null,
      displayOrder: 1,
      configuration: { kind: 'Text' },
      style: {},
      options: [],
      usageCount: 0,
      ...values,
    };
  }
}
describe('date values', () => {
  it('round-trips a date without shifting the day', () => {
    const date = new Date(2026, 0, 31);
    expect(CellValueUtil.toDateValue(date)).toBe('2026-01-31');
    expect(CellValueUtil.fromDateValue('2026-01-31')?.getDate()).toBe(31);
  });

  it('treats an empty value as no date', () => {
    expect(CellValueUtil.fromDateValue(null)).toBeNull();
  });
});

describe('valueRequest', () => {
  it('sets the value in the shape the cell kind takes', () => {
    expect(CellValueUtil.valueRequest(SpecHelpers.cell(), 'Text', 'abc')).toEqual({ sheetCellId: 7, value: 'abc' });
    expect(CellValueUtil.valueRequest(SpecHelpers.cell(), 'Number', 1.5)).toEqual({ sheetCellId: 7, value: 1.5 });
    expect(CellValueUtil.valueRequest(SpecHelpers.cell(), 'Checkbox', true)).toEqual({ sheetCellId: 7, value: true });
    expect(CellValueUtil.valueRequest(SpecHelpers.cell(), 'TextDropdown', 3)).toEqual({ sheetCellId: 7, value: 3 });
    expect(CellValueUtil.valueRequest(SpecHelpers.cell(), 'LinkedDropdown', 'P-1001')).toEqual({
      sheetCellId: 7,
      value: 'P-1001',
    });
    expect(CellValueUtil.valueRequest(SpecHelpers.cell(), 'Date', new Date(2026, 5, 2))).toEqual({
      sheetCellId: 7,
      value: '2026-06-02',
    });
  });

  it('clears the cell when the value is null', () => {
    expect(CellValueUtil.valueRequest(SpecHelpers.cell(), 'Number', null)).toEqual({ sheetCellId: 7, value: null });
  });
});

describe('displayValue', () => {
  it('shows text and dates as they are stored', () => {
    expect(CellValueUtil.displayValue(SpecHelpers.cell({ value: 'abc' }), SpecHelpers.cellType(), { kind: 'Text' })).toBe('abc');
    expect(
      CellValueUtil.displayValue(SpecHelpers.cell({ value: '2026-06-02' }), SpecHelpers.cellType({ kind: 'Date' }), { kind: 'Date' }),
    ).toBe('2026-06-02');
  });

  it('shows a number to its decimal places, followed by its unit', () => {
    const number = SpecHelpers.cellType({ kind: 'Number' });

    expect(CellValueUtil.displayValue(SpecHelpers.cell({ value: 12.5 }), number, { kind: 'Number' })).toBe('12.5');
    expect(
      CellValueUtil.displayValue(SpecHelpers.cell({ value: 12.5 }), number, { kind: 'Number', decimalPlaces: 2 }),
    ).toBe('12.50');
    expect(
      CellValueUtil.displayValue(SpecHelpers.cell({ value: 3 }), number, {
        kind: 'Number',
        decimalPlaces: 1,
        unit: 'Nm',
      }),
    ).toBe('3.0 Nm');
  });

  it('shows the chosen option of a dropdown', () => {
    const dropdown = SpecHelpers.cellType({
      kind: 'TextDropdown',
      options: [
        { id: 4, value: 'Steel', displayOrder: 1 },
        { id: 5, value: 'Alloy', displayOrder: 2 },
      ],
    });

    expect(CellValueUtil.displayValue(SpecHelpers.cell({ value: 5 }), dropdown, { kind: 'TextDropdown' })).toBe('Alloy');
    expect(CellValueUtil.displayValue(SpecHelpers.cell({ value: 9 }), dropdown, { kind: 'TextDropdown' })).toBe('');
  });

  it('shows the text chosen in a linked dropdown, which is stored as it was picked', () => {
    const linked = SpecHelpers.cellType({ kind: 'LinkedDropdown' });

    expect(CellValueUtil.displayValue(SpecHelpers.cell({ value: 'P-1001' }), linked, { kind: 'LinkedDropdown' })).toBe(
      'P-1001',
    );
  });

  it('shows nothing for an empty cell, or for a kind that has no text', () => {
    expect(CellValueUtil.displayValue(SpecHelpers.cell(), SpecHelpers.cellType(), { kind: 'Text' })).toBe('');
    expect(CellValueUtil.displayValue(SpecHelpers.cell(), SpecHelpers.cellType({ kind: 'Number' }), { kind: 'Number', unit: 'Nm' })).toBe(
      '',
    );
    expect(
      CellValueUtil.displayValue(SpecHelpers.cell({ value: true }), SpecHelpers.cellType({ kind: 'Checkbox' }), {
        kind: 'Checkbox',
      }),
    ).toBe('');
  });
});
