import {
  LinkedDropdownUtil } from './linked-dropdown.util';
import { LinkedChoices } from '../models/linked-choices.model';
import { LinkedDropdownInstanceSettings } from '../models/linked-dropdown-instance-settings.model';
import { Sheet } from '../models/sheet.model';
import { SheetIndexUtil } from '../layout/sheet-index.util';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';

class SpecHelpers {
  public static sheetWithParts(options: string[] | null = ['P-1001', 'P-1002']): Sheet {
    const table = SheetStructureFixture.fixtureTable();
    table.title = 'Piston parts';
    table.linkableColumns = [{ templateCellId: PART_NUMBER, label: 'Part number' }];
    table.linkableRows = [{ sheetRowId: PIN_OFFSET_ROW, label: 'Pin offset (mm)' }];
    return {
      id: 5,
      publicId: 'sheet-5',
      phaseId: 1,
      sheetTypeId: 2,
      isLive: true,
      viewedVersionNumber: null,
      viewedAsOfUtc: null,
      latestVersionNumber: null,
      myDraftCount: 0,
      otherDrafts: [],
      versions: [],
      tables: [table],
      availableTemplates: [],
      linkedSources:
        options === null
          ? []
          : [
              { sheetTableId: table.id, templateCellId: PART_NUMBER, sheetRowId: null, options },
              {
                sheetTableId: table.id,
                templateCellId: null,
                sheetRowId: PIN_OFFSET_ROW,
                options: ['0.8', '0.85'],
              },
            ],
    };
  }

  public static pointedAt(sourceSheetTableId: number | null, sourceTemplateCellId: number | null): LinkedDropdownInstanceSettings {
    return { kind: 'LinkedDropdown', sourceSheetTableId, sourceTemplateCellId };
  }

  public static choicesFor(sheet: Sheet, settings: LinkedDropdownInstanceSettings | null): LinkedChoices {
    return LinkedDropdownUtil.linkedChoices(sheet, SheetIndexUtil.buildSheetIndex(sheet), settings);
  }
}

const PART_NUMBER = 15;
const PIN_OFFSET_ROW = 70;
describe('linkedChoices', () => {
  it('offers the values of the column the cell is pointed at, named by its table and column', () => {
    const sheet = SpecHelpers.sheetWithParts();

    expect(SpecHelpers.choicesFor(sheet, SpecHelpers.pointedAt(1, PART_NUMBER))).toEqual({
      status: 'ready',
      source: 'Piston parts › Part number',
      options: ['P-1001', 'P-1002'],
    });
  });

  it('offers the values of the row the cell is pointed at, named by its table and the row', () => {
    const sheet = SpecHelpers.sheetWithParts();
    const settings = { ...SpecHelpers.pointedAt(1, null), sourceSheetRowId: PIN_OFFSET_ROW };

    expect(SpecHelpers.choicesFor(sheet, settings)).toEqual({
      status: 'ready',
      source: 'Piston parts › Pin offset (mm)',
      options: ['0.8', '0.85'],
    });
  });

  it('is missing when the row it was pointed at is no longer on the sheet', () => {
    const settings = { ...SpecHelpers.pointedAt(1, null), sourceSheetRowId: 71 };

    expect(SpecHelpers.choicesFor(SpecHelpers.sheetWithParts(), settings).status).toBe('missing');
  });

  it('is unset when it is given both a column and a row', () => {
    const settings = { ...SpecHelpers.pointedAt(1, PART_NUMBER), sourceSheetRowId: PIN_OFFSET_ROW };

    expect(SpecHelpers.choicesFor(SpecHelpers.sheetWithParts(), settings).status).toBe('unset');
  });

  it('is unset until both a table and a column are chosen', () => {
    const sheet = SpecHelpers.sheetWithParts();

    expect(SpecHelpers.choicesFor(sheet, null).status).toBe('unset');
    expect(SpecHelpers.choicesFor(sheet, SpecHelpers.pointedAt(1, null)).status).toBe('unset');
    expect(SpecHelpers.choicesFor(sheet, SpecHelpers.pointedAt(null, PART_NUMBER)).status).toBe('unset');
  });

  it('is missing when the table or the column is no longer on the sheet', () => {
    const sheet = SpecHelpers.sheetWithParts();

    expect(SpecHelpers.choicesFor(sheet, SpecHelpers.pointedAt(99, PART_NUMBER)).status).toBe('missing');
    expect(SpecHelpers.choicesFor(sheet, SpecHelpers.pointedAt(1, 99)).status).toBe('missing');
  });

  it('is ready with nothing to offer when the column has no values yet', () => {
    const choices = SpecHelpers.choicesFor(SpecHelpers.sheetWithParts(null), SpecHelpers.pointedAt(1, PART_NUMBER));

    expect(choices.status).toBe('ready');
    expect(choices.options).toEqual([]);
  });

  it('counts two answers as the same only when they offer the very same list', () => {
    const sheet = SpecHelpers.sheetWithParts();
    const first = SpecHelpers.choicesFor(sheet, SpecHelpers.pointedAt(1, PART_NUMBER));
    const again = SpecHelpers.choicesFor(sheet, SpecHelpers.pointedAt(1, PART_NUMBER));
    const afterAChange = SpecHelpers.choicesFor(SpecHelpers.sheetWithParts(['P-1001']), SpecHelpers.pointedAt(1, PART_NUMBER));

    expect(LinkedDropdownUtil.sameLinkedChoices(first, again)).toBe(true);
    expect(LinkedDropdownUtil.sameLinkedChoices(first, afterAChange)).toBe(false);
    expect(LinkedDropdownUtil.sameLinkedChoices(first, null)).toBe(false);
    expect(LinkedDropdownUtil.sameLinkedChoices(null, null)).toBe(true);
  });
});

describe('a linked dropdown cell', () => {
  const ready: LinkedChoices = {
    status: 'ready',
    source: 'Piston parts › Part number',
    options: ['P-1001', 'P-1002'],
  };
  const unset: LinkedChoices = { status: 'unset', source: '', options: [] };
  const missing: LinkedChoices = { status: 'missing', source: '', options: [] };

  it('needs no attention while its value is one of the choices, or it has none', () => {
    expect(LinkedDropdownUtil.linkedWarning(ready, 'P-1001')).toBeNull();
    expect(LinkedDropdownUtil.linkedWarning(ready, null)).toBeNull();
    expect(LinkedDropdownUtil.linkedWarning(unset, null)).toBeNull();
  });

  it('is flagged when its value is no longer in the column, and keeps the value', () => {
    expect(LinkedDropdownUtil.linkedWarning(ready, 'P-0999')).toContain('"P-0999" is no longer in Piston parts');
    expect(LinkedDropdownUtil.linkedOptions(ready, 'P-0999')).toEqual(['P-0999', 'P-1001', 'P-1002']);
    expect(LinkedDropdownUtil.linkedOptions(ready, 'P-1001')).toBe(ready.options);
  });

  it('is flagged when what it was pointed at has gone', () => {
    expect(LinkedDropdownUtil.linkedWarning(missing, 'P-1001')).toContain('no longer on the sheet');
  });

  it('says why it has nothing to choose from', () => {
    expect(LinkedDropdownUtil.linkedHint(unset)).toBe('No source chosen');
    expect(LinkedDropdownUtil.linkedHint(missing)).toBe('Source removed');
    expect(LinkedDropdownUtil.linkedHint(ready)).toBeNull();
  });
});
