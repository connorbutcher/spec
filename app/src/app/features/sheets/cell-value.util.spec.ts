import { CellType } from '../templates/models/cell-type.model';
import { displayValue, fromDateValue, toDateValue, valueRequest } from './cell-value.util';
import { SheetCell } from './models/sheet-cell.model';

function cell(values: Partial<SheetCell> = {}): SheetCell {
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
    textValue: null,
    numberValue: null,
    dateValue: null,
    booleanValue: null,
    optionId: null,
    settings: null,
    sheetColumnBlockId: null,
    lastChange: null,
    ...values,
  };
}

function cellType(values: Partial<CellType> = {}): CellType {
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

describe('date values', () => {
  it('round-trips a date without shifting the day', () => {
    const date = new Date(2026, 0, 31);
    expect(toDateValue(date)).toBe('2026-01-31');
    expect(fromDateValue('2026-01-31')?.getDate()).toBe(31);
  });

  it('treats an empty value as no date', () => {
    expect(fromDateValue(null)).toBeNull();
  });
});

describe('valueRequest', () => {
  it('sets the field the cell kind uses', () => {
    expect(valueRequest(cell(), 'Text', 'abc')).toEqual({ sheetCellId: 7, text: 'abc' });
    expect(valueRequest(cell(), 'Number', 1.5)).toEqual({ sheetCellId: 7, number: 1.5 });
    expect(valueRequest(cell(), 'Checkbox', true)).toEqual({ sheetCellId: 7, boolean: true });
    expect(valueRequest(cell(), 'TextDropdown', 3)).toEqual({ sheetCellId: 7, optionId: 3 });
    expect(valueRequest(cell(), 'LinkedDropdown', 'P-1001')).toEqual({
      sheetCellId: 7,
      text: 'P-1001',
    });
    expect(valueRequest(cell(), 'Date', new Date(2026, 5, 2))).toEqual({
      sheetCellId: 7,
      date: '2026-06-02',
    });
  });

  it('clears the cell when the value is null', () => {
    expect(valueRequest(cell(), 'Number', null)).toEqual({ sheetCellId: 7, number: null });
  });
});

describe('displayValue', () => {
  it('shows text and dates as they are stored', () => {
    expect(displayValue(cell({ textValue: 'abc' }), cellType(), { kind: 'Text' })).toBe('abc');
    expect(
      displayValue(cell({ dateValue: '2026-06-02' }), cellType({ kind: 'Date' }), { kind: 'Date' }),
    ).toBe('2026-06-02');
  });

  it('shows a number to its decimal places, followed by its unit', () => {
    const number = cellType({ kind: 'Number' });

    expect(displayValue(cell({ numberValue: 12.5 }), number, { kind: 'Number' })).toBe('12.5');
    expect(
      displayValue(cell({ numberValue: 12.5 }), number, { kind: 'Number', decimalPlaces: 2 }),
    ).toBe('12.50');
    expect(
      displayValue(cell({ numberValue: 3 }), number, {
        kind: 'Number',
        decimalPlaces: 1,
        unit: 'Nm',
      }),
    ).toBe('3.0 Nm');
  });

  it('shows the chosen option of a dropdown', () => {
    const dropdown = cellType({
      kind: 'TextDropdown',
      options: [
        { id: 4, value: 'Steel', displayOrder: 1 },
        { id: 5, value: 'Alloy', displayOrder: 2 },
      ],
    });

    expect(displayValue(cell({ optionId: 5 }), dropdown, { kind: 'TextDropdown' })).toBe('Alloy');
    expect(displayValue(cell({ optionId: 9 }), dropdown, { kind: 'TextDropdown' })).toBe('');
  });

  it('shows the text chosen in a linked dropdown, which is stored as it was picked', () => {
    const linked = cellType({ kind: 'LinkedDropdown' });

    expect(displayValue(cell({ textValue: 'P-1001' }), linked, { kind: 'LinkedDropdown' })).toBe(
      'P-1001',
    );
  });

  it('shows nothing for an empty cell, or for a kind that has no text', () => {
    expect(displayValue(cell(), cellType(), { kind: 'Text' })).toBe('');
    expect(displayValue(cell(), cellType({ kind: 'Number' }), { kind: 'Number', unit: 'Nm' })).toBe(
      '',
    );
    expect(
      displayValue(cell({ booleanValue: true }), cellType({ kind: 'Checkbox' }), {
        kind: 'Checkbox',
      }),
    ).toBe('');
  });
});
