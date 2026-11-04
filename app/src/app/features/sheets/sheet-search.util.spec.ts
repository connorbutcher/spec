import { CellConfiguration } from '../templates/models/cell-configuration';
import { CellKind } from '../templates/models/cell-kind';
import { CellType } from '../templates/models/cell-type.model';
import { SheetCell } from './models/sheet-cell.model';
import { SheetTable } from './models/sheet-table.model';
import { Sheet } from './models/sheet.model';
import { SheetIndexUtil } from './sheet-index.util';
import { SheetAnchorUtil } from './sheet-anchor.util';
import { SheetSearchUtil } from './sheet-search.util';
import { SheetStructureFixture } from './sheet-structure.fixture';

class SpecHelpers {
  public static cellType(id: number, kind: CellKind, configuration: CellConfiguration): CellType {
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

  /** The header row's five cells: a heading, then text, a number, a dropdown and a date, all filled in. */
  public static filledTable(): { table: SheetTable; cells: SheetCell[] } {
    const table = SheetStructureFixture.fixtureTable();
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

  public static search(table: SheetTable, query: string): string[] {
    const sheet = SpecHelpers.sheetOf(table);
    return SheetSearchUtil.matchingKeys(SheetSearchUtil.searchTargets(sheet, SheetIndexUtil.buildSheetIndex(sheet), TYPES), query);
  }

  private static sheetOf(table: SheetTable): Sheet {
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
}

const TYPES: ReadonlyMap<number, CellType> = new Map(
  [
    SpecHelpers.cellType(1, 'Text', { kind: 'Text' }),
    SpecHelpers.cellType(2, 'Number', { kind: 'Number', decimalPlaces: 2, unit: 'mm' }),
    SpecHelpers.cellType(3, 'TextDropdown', { kind: 'TextDropdown' }),
    SpecHelpers.cellType(4, 'Heading', { kind: 'Heading' }),
    SpecHelpers.cellType(5, 'Date', { kind: 'Date' }),
  ].map((type) => [type.id, type]),
);
describe('searchTargets and matchingKeys', () => {
  it('finds a table by its title or its template, ignoring case', () => {
    const { table } = SpecHelpers.filledTable();

    expect(SpecHelpers.search(table, 'PISTON')).toEqual([SheetAnchorUtil.anchorKey('table', table.publicId)]);
    expect(SpecHelpers.search(table, 'limits table')).toEqual([SheetAnchorUtil.anchorKey('table', table.publicId)]);
  });

  it('finds cells by what they show: captions, text, numbers, option text and dates', () => {
    const { table, cells } = SpecHelpers.filledTable();

    expect(SpecHelpers.search(table, 'bore')).toEqual([
      SheetAnchorUtil.anchorKey('cell', cells[0].publicId),
      SheetAnchorUtil.anchorKey('cell', cells[1].publicId),
    ]);
    expect(SpecHelpers.search(table, '12.50 mm')).toEqual([SheetAnchorUtil.anchorKey('cell', cells[2].publicId)]);
    expect(SpecHelpers.search(table, 'forged')).toEqual([SheetAnchorUtil.anchorKey('cell', cells[3].publicId)]);
    expect(SpecHelpers.search(table, '2026-11')).toEqual([SheetAnchorUtil.anchorKey('cell', cells[4].publicId)]);
  });

  it('finds nothing for an option id that is never shown, or for a blank query', () => {
    const { table } = SpecHelpers.filledTable();

    expect(SpecHelpers.search(table, '4')).toEqual([]);
    expect(SpecHelpers.search(table, '   ')).toEqual([]);
  });
});

describe('matchCountLabel', () => {
  it('counts from one, and says when nothing was found', () => {
    expect(SheetSearchUtil.matchCountLabel(2, 12)).toBe('3 of 12');
    expect(SheetSearchUtil.matchCountLabel(-1, 0)).toBe('No matches');
  });
});
