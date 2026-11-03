import { Sheet } from './models/sheet.model';
import {
  anchorId,
  anchorKey,
  anchorPublicId,
  cellRowId,
  parseAnchor,
  selectionAnchor,
} from './sheet-anchor.util';
import { buildSheetIndex } from './sheet-index.util';
import { fixtureTable } from './sheet-structure.fixture';

function fixtureSheet(): Sheet {
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
    tables: [fixtureTable()],
    availableTemplates: [],
    linkedSources: [],
  };
}

describe('parseAnchor', () => {
  it('reads the kind and the public id, which may hold dashes of its own', () => {
    expect(parseAnchor('row-b7d132de-ecc0-f111')).toEqual({
      kind: 'row',
      publicId: 'b7d132de-ecc0-f111',
    });
    expect(parseAnchor(anchorKey('cell', 'c-1'))).toEqual({ kind: 'cell', publicId: 'c-1' });
  });

  it('is null for no fragment, an unknown kind, or no id', () => {
    expect(parseAnchor(null)).toBeNull();
    expect(parseAnchor('')).toBeNull();
    expect(parseAnchor('column-12')).toBeNull();
    expect(parseAnchor('row-')).toBeNull();
    expect(parseAnchor('row')).toBeNull();
  });
});

describe('anchorId and anchorPublicId', () => {
  it('go between sheet ids and public ids, for each kind', () => {
    const sheet = fixtureSheet();
    const index = buildSheetIndex(sheet);
    const table = sheet.tables[0];
    const section = table.sections[0];
    const row = section.rows[0];
    const cell = row.cells[0];

    expect(anchorPublicId(index, 'table', table.id)).toBe(table.publicId);
    expect(anchorId(index, { kind: 'section', publicId: section.publicId })).toBe(section.id);
    expect(anchorId(index, { kind: 'row', publicId: row.publicId })).toBe(row.id);
    expect(anchorId(index, { kind: 'cell', publicId: cell.publicId })).toBe(cell.id);
    expect(cellRowId(index, cell.id)).toBe(row.id);
  });

  it('find nothing for what is not on the sheet', () => {
    const index = buildSheetIndex(fixtureSheet());

    expect(anchorId(index, { kind: 'row', publicId: 'gone' })).toBeNull();
    expect(anchorPublicId(index, 'cell', 999_999)).toBeNull();
  });
});

describe('selectionAnchor', () => {
  it('names the row, else the section, else the table', () => {
    const sheet = fixtureSheet();
    const index = buildSheetIndex(sheet);
    const table = sheet.tables[0];
    const section = table.sections[0];
    const row = section.rows[0];

    expect(
      selectionAnchor(index, { tableId: table.id, sectionId: section.id, rowId: row.id }),
    ).toEqual({ kind: 'row', publicId: row.publicId });
    expect(
      selectionAnchor(index, { tableId: table.id, sectionId: section.id, rowId: null }),
    ).toEqual({ kind: 'section', publicId: section.publicId });
    expect(selectionAnchor(index, { tableId: table.id, sectionId: null, rowId: null })).toEqual({
      kind: 'table',
      publicId: table.publicId,
    });
    expect(selectionAnchor(index, null)).toBeNull();
  });
});
