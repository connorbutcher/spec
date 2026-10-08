import {
  linkedChoices,
  linkedHint,
  linkedOptions,
  linkedWarning,
  sameLinkedChoices,
} from './linked-dropdown.util';
import { LinkedChoices } from './models/linked-choices.model';
import { LinkedDropdownInstanceSettings } from './models/linked-dropdown-instance-settings.model';
import { Sheet } from './models/sheet.model';
import { buildSheetIndex } from './sheet-index.util';
import { fixtureTable } from './sheet-structure.fixture';

const PART_NUMBER = 15;

function sheetWithParts(options: string[] | null = ['P-1001', 'P-1002']): Sheet {
  const table = fixtureTable();
  table.title = 'Piston parts';
  table.linkableColumns = [{ templateCellId: PART_NUMBER, label: 'Part number' }];
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
        : [{ sheetTableId: table.id, templateCellId: PART_NUMBER, options }],
  };
}

function pointedAt(
  sourceSheetTableId: number | null,
  sourceTemplateCellId: number | null,
): LinkedDropdownInstanceSettings {
  return { kind: 'LinkedDropdown', sourceSheetTableId, sourceTemplateCellId };
}

function choicesFor(sheet: Sheet, settings: LinkedDropdownInstanceSettings | null): LinkedChoices {
  return linkedChoices(sheet, buildSheetIndex(sheet), settings);
}

describe('linkedChoices', () => {
  it('offers the values of the column the cell is pointed at, named by its table and column', () => {
    const sheet = sheetWithParts();

    expect(choicesFor(sheet, pointedAt(1, PART_NUMBER))).toEqual({
      status: 'ready',
      source: 'Piston parts › Part number',
      options: ['P-1001', 'P-1002'],
    });
  });

  it('is unset until both a table and a column are chosen', () => {
    const sheet = sheetWithParts();

    expect(choicesFor(sheet, null).status).toBe('unset');
    expect(choicesFor(sheet, pointedAt(1, null)).status).toBe('unset');
    expect(choicesFor(sheet, pointedAt(null, PART_NUMBER)).status).toBe('unset');
  });

  it('is missing when the table or the column is no longer on the sheet', () => {
    const sheet = sheetWithParts();

    expect(choicesFor(sheet, pointedAt(99, PART_NUMBER)).status).toBe('missing');
    expect(choicesFor(sheet, pointedAt(1, 99)).status).toBe('missing');
  });

  it('is ready with nothing to offer when the column has no values yet', () => {
    const choices = choicesFor(sheetWithParts(null), pointedAt(1, PART_NUMBER));

    expect(choices.status).toBe('ready');
    expect(choices.options).toEqual([]);
  });

  it('counts two answers as the same only when they offer the very same list', () => {
    const sheet = sheetWithParts();
    const first = choicesFor(sheet, pointedAt(1, PART_NUMBER));
    const again = choicesFor(sheet, pointedAt(1, PART_NUMBER));
    const afterAChange = choicesFor(sheetWithParts(['P-1001']), pointedAt(1, PART_NUMBER));

    expect(sameLinkedChoices(first, again)).toBe(true);
    expect(sameLinkedChoices(first, afterAChange)).toBe(false);
    expect(sameLinkedChoices(first, null)).toBe(false);
    expect(sameLinkedChoices(null, null)).toBe(true);
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
    expect(linkedWarning(ready, 'P-1001')).toBeNull();
    expect(linkedWarning(ready, null)).toBeNull();
    expect(linkedWarning(unset, null)).toBeNull();
  });

  it('is flagged when its value is no longer in the column, and keeps the value', () => {
    expect(linkedWarning(ready, 'P-0999')).toContain('"P-0999" is no longer in Piston parts');
    expect(linkedOptions(ready, 'P-0999')).toEqual(['P-0999', 'P-1001', 'P-1002']);
    expect(linkedOptions(ready, 'P-1001')).toBe(ready.options);
  });

  it('is flagged when what it was pointed at has gone', () => {
    expect(linkedWarning(missing, 'P-1001')).toContain('no longer on the sheet');
  });

  it('says why it has nothing to choose from', () => {
    expect(linkedHint(unset)).toBe('No source chosen');
    expect(linkedHint(missing)).toBe('Source removed');
    expect(linkedHint(ready)).toBeNull();
  });
});
