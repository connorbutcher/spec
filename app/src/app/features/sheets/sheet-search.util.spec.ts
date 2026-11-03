import { CellConfiguration } from '../templates/models/cell-configuration';
import { CellKind } from '../templates/models/cell-kind';
import { CellType } from '../templates/models/cell-type.model';
import { SheetCell } from './models/sheet-cell.model';
import { SheetTable } from './models/sheet-table.model';
import { Sheet } from './models/sheet.model';
import { buildSheetIndex } from './sheet-index.util';
import { anchorKey } from './sheet-anchor.util';
import { matchCountLabel, matchingKeys, searchTargets } from './sheet-search.util';
import { fixtureTable } from './sheet-structure.fixture';

function cellType(id: number, kind: CellKind, configuration: CellConfiguration): CellType {
  return {
    id,
    name: kind,
    kind,
    description: null,
    displayOrder: id,
    configuration,
    style: {},
    options: kind === 'TextDropdown' ? [{ id: 4, value: 'Forged steel', displayOrder: 1 }] : [],
    usageCount: 0,
  };
}

const TYPES: ReadonlyMap<number, CellType> = new Map(
  [
    cellType(1, 'Text', { kind: 'Text' }),
    cellType(2, 'Number', { kind: 'Number', decimalPlaces: 2, unit: 'mm' }),
    cellType(3, 'TextDropdown', { kind: 'TextDropdown' }),
    cellType(4, 'Heading', { kind: 'Heading' }),
    cellType(5, 'Date', { kind: 'Date' }),
  ].map((type) => [type.id, type]),
);

function sheetOf(table: SheetTable): Sheet {
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
    linkedSources: [],
  };
}

/** The header row's five cells: a heading, then text, a number, a dropdown and a date, all filled in. */
function filledTable(): { table: SheetTable; cells: SheetCell[] } {
  const table = fixtureTable();
  table.title = 'Piston limits';
  const cells = table.sections.find((section) => section.role === 'Header')?.rows[0]
    .cells as SheetCell[];
  cells[0].template = { ...cells[0].template, cellTypeId: 4, caption: 'Bore diameter' };
  cells[1].template = { ...cells[1].template, cellTypeId: 1 };
  cells[1].textValue = 'Bore A';
  cells[2].template = { ...cells[2].template, cellTypeId: 2 };
  cells[2].numberValue = 12.5;
  cells[3].template = { ...cells[3].template, cellTypeId: 3 };
  cells[3].optionId = 4;
  cells[4].template = { ...cells[4].template, cellTypeId: 5 };
  cells[4].dateValue = '2026-11-03';
  return { table, cells };
}

function search(table: SheetTable, query: string): string[] {
  const sheet = sheetOf(table);
  return matchingKeys(searchTargets(sheet, buildSheetIndex(sheet), TYPES), query);
}

describe('searchTargets and matchingKeys', () => {
  it('finds a table by its title or its template, ignoring case', () => {
    const { table } = filledTable();

    expect(search(table, 'PISTON')).toEqual([anchorKey('table', table.publicId)]);
    expect(search(table, 'limits table')).toEqual([anchorKey('table', table.publicId)]);
  });

  it('finds cells by what they show: captions, text, numbers, option text and dates', () => {
    const { table, cells } = filledTable();

    expect(search(table, 'bore')).toEqual([
      anchorKey('cell', cells[0].publicId),
      anchorKey('cell', cells[1].publicId),
    ]);
    expect(search(table, '12.50 mm')).toEqual([anchorKey('cell', cells[2].publicId)]);
    expect(search(table, 'forged')).toEqual([anchorKey('cell', cells[3].publicId)]);
    expect(search(table, '2026-11')).toEqual([anchorKey('cell', cells[4].publicId)]);
  });

  it('finds nothing for an option id that is never shown, or for a blank query', () => {
    const { table } = filledTable();

    expect(search(table, '4')).toEqual([]);
    expect(search(table, '   ')).toEqual([]);
  });
});

describe('matchCountLabel', () => {
  it('counts from one, and says when nothing was found', () => {
    expect(matchCountLabel(2, 12)).toBe('3 of 12');
    expect(matchCountLabel(-1, 0)).toBe('No matches');
  });
});
