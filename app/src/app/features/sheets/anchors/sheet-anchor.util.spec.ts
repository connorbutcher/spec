import { Sheet } from '../models/sheet.model';
import {
  SheetAnchorUtil } from './sheet-anchor.util';
import { SheetIndexUtil } from '../layout/sheet-index.util';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';

class SpecHelpers {
  public static fixtureSheet(): Sheet {
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
      tables: [SheetStructureFixture.fixtureTable()],
      availableTemplates: [],
      linkedSources: [],
    };
  }
}
describe('parseAnchor', () => {
  it('reads the kind and the public id, which may hold dashes of its own', () => {
    expect(SheetAnchorUtil.parseAnchor('row-b7d132de-ecc0-f111')).toEqual({
      kind: 'row',
      publicId: 'b7d132de-ecc0-f111',
    });
    expect(SheetAnchorUtil.parseAnchor(SheetAnchorUtil.anchorKey('cell', 'c-1'))).toEqual({ kind: 'cell', publicId: 'c-1' });
  });

  it('is null for no fragment, an unknown kind, or no id', () => {
    expect(SheetAnchorUtil.parseAnchor(null)).toBeNull();
    expect(SheetAnchorUtil.parseAnchor('')).toBeNull();
    expect(SheetAnchorUtil.parseAnchor('column-12')).toBeNull();
    expect(SheetAnchorUtil.parseAnchor('row-')).toBeNull();
    expect(SheetAnchorUtil.parseAnchor('row')).toBeNull();
  });
});

describe('anchorId and anchorPublicId', () => {
  it('go between sheet ids and public ids, for each kind', () => {
    const sheet = SpecHelpers.fixtureSheet();
    const index = SheetIndexUtil.buildSheetIndex(sheet);
    const table = sheet.tables[0];
    const section = table.sections[0];
    const row = section.rows[0];
    const cell = row.cells[0];

    expect(SheetAnchorUtil.anchorPublicId(index, 'table', table.id)).toBe(table.publicId);
    expect(SheetAnchorUtil.anchorId(index, { kind: 'section', publicId: section.publicId })).toBe(section.id);
    expect(SheetAnchorUtil.anchorId(index, { kind: 'row', publicId: row.publicId })).toBe(row.id);
    expect(SheetAnchorUtil.anchorId(index, { kind: 'cell', publicId: cell.publicId })).toBe(cell.id);
    expect(SheetAnchorUtil.cellRowId(index, cell.id)).toBe(row.id);
  });

  it('find nothing for what is not on the sheet', () => {
    const index = SheetIndexUtil.buildSheetIndex(SpecHelpers.fixtureSheet());

    expect(SheetAnchorUtil.anchorId(index, { kind: 'row', publicId: 'gone' })).toBeNull();
    expect(SheetAnchorUtil.anchorPublicId(index, 'cell', 999_999)).toBeNull();
  });
});

describe('selectionAnchor', () => {
  it('names the row, else the section, else the table', () => {
    const sheet = SpecHelpers.fixtureSheet();
    const index = SheetIndexUtil.buildSheetIndex(sheet);
    const table = sheet.tables[0];
    const section = table.sections[0];
    const row = section.rows[0];

    expect(
      SheetAnchorUtil.selectionAnchor(index, { tableId: table.id, sectionId: section.id, rowId: row.id }),
    ).toEqual({ kind: 'row', publicId: row.publicId });
    expect(
      SheetAnchorUtil.selectionAnchor(index, { tableId: table.id, sectionId: section.id, rowId: null }),
    ).toEqual({ kind: 'section', publicId: section.publicId });
    expect(SheetAnchorUtil.selectionAnchor(index, { tableId: table.id, sectionId: null, rowId: null })).toEqual({
      kind: 'table',
      publicId: table.publicId,
    });
    expect(SheetAnchorUtil.selectionAnchor(index, null)).toBeNull();
  });
});
