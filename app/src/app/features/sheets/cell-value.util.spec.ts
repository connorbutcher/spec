import { fromDateValue, toDateValue, valueRequest } from './cell-value.util';
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
    },
    textValue: null,
    numberValue: null,
    dateValue: null,
    booleanValue: null,
    optionId: null,
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
    expect(valueRequest(cell(), 'Date', new Date(2026, 5, 2))).toEqual({
      sheetCellId: 7,
      date: '2026-06-02',
    });
  });

  it('clears the cell when the value is null', () => {
    expect(valueRequest(cell(), 'Number', null)).toEqual({ sheetCellId: 7, number: null });
  });
});
