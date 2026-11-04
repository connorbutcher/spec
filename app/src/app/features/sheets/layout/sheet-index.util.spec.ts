import {
  SheetIndexUtil } from './sheet-index.util';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';
import { Sheet } from '../models/sheet.model';

class SpecHelpers {
  public static sheetWithTable(): Sheet {
    return {
      id: 1,
      publicId: 'sheet-1',
      phaseId: 1,
      sheetTypeId: 1,
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
describe('sheet index', () => {
  const sheet = SpecHelpers.sheetWithTable();
  const index = SheetIndexUtil.buildSheetIndex(sheet);
  const group = sheet.tables[0].sections.find((section) => section.name === 'Group');
  const limits = group?.sections[0];

  it('finds every section, row and cell by id and remembers where each sits', () => {
    expect(index.sections.size).toBe(3);
    expect(index.rows.size).toBe(3);
    expect(index.cells.size).toBe(11);
    expect(index.sectionTable.get(limits?.id ?? -1)).toBe(1);
    expect(index.sectionParent.get(limits?.id ?? -1)).toBe(group?.id);
    expect(index.sectionParent.get(group?.id ?? -1)).toBeNull();
  });

  it('lists a section ancestors from the top down', () => {
    expect(SheetIndexUtil.sectionAncestors(index, limits?.id ?? -1).map((section) => section.name)).toEqual([
      'Group',
    ]);
    expect(SheetIndexUtil.sectionAncestors(index, group?.id ?? -1)).toEqual([]);
  });

  it('lists the sections at the same level, itself included', () => {
    expect(SheetIndexUtil.sectionSiblings(index, limits?.id ?? -1).map((section) => section.name)).toEqual([
      'Limits',
    ]);
    expect(SheetIndexUtil.sectionSiblings(index, group?.id ?? -1).map((section) => section.name)).toEqual([
      'Group',
      'Header',
    ]);
  });

  it('counts a section as a group when it holds, or can hold, other sections', () => {
    const header = sheet.tables[0].sections.find((section) => section.role === 'Header');

    expect(group && SheetIndexUtil.isGroupSection(group)).toBe(true);
    expect(limits && SheetIndexUtil.isGroupSection(limits)).toBe(false);
    expect(header && SheetIndexUtil.isGroupSection(header)).toBe(false);
    expect(
      limits &&
        SheetIndexUtil.isGroupSection({
          ...limits,
          addableSections: [
            {
              templateSectionId: 1,
              name: 'Sub',
              count: 0,
              minInstances: 0,
              maxInstances: null,
              canAdd: true,
            },
          ],
        }),
    ).toBe(true);
  });

  it('gives a row the tone of its section: header, group by depth, or plain', () => {
    const header = sheet.tables[0].sections.find((section) => section.role === 'Header');

    expect(SheetIndexUtil.sectionTone(index, header?.rows[0].id ?? -1)).toBe('header');
    expect(SheetIndexUtil.sectionTone(index, group?.rows[0].id ?? -1)).toBe('group-0');
    expect(SheetIndexUtil.sectionTone(index, limits?.rows[0].id ?? -1)).toBe('plain');
    expect(SheetIndexUtil.sectionTone(index, -1)).toBe('plain');
  });

  it('picks out cells by id, ignoring ids the sheet does not have', () => {
    const first = limits?.rows[0].cells[0];

    expect([...SheetIndexUtil.cellsById(sheet, [first?.id ?? -1, -5])]).toEqual([first]);
    expect(SheetIndexUtil.cellsById(sheet, []).size).toBe(0);
  });
});
