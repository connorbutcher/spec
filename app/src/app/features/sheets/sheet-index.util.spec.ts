import {
  buildSheetIndex,
  cellsById,
  isGroupSection,
  sectionAncestors,
  sectionSiblings,
  sectionTone,
} from './sheet-index.util';
import { fixtureTable } from './sheet-structure.fixture';
import { Sheet } from './models/sheet.model';

function sheetWithTable(): Sheet {
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
    tables: [fixtureTable()],
    availableTemplates: [],
  };
}

describe('sheet index', () => {
  const sheet = sheetWithTable();
  const index = buildSheetIndex(sheet);
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
    expect(sectionAncestors(index, limits?.id ?? -1).map((section) => section.name)).toEqual([
      'Group',
    ]);
    expect(sectionAncestors(index, group?.id ?? -1)).toEqual([]);
  });

  it('lists the sections at the same level, itself included', () => {
    expect(sectionSiblings(index, limits?.id ?? -1).map((section) => section.name)).toEqual([
      'Limits',
    ]);
    expect(sectionSiblings(index, group?.id ?? -1).map((section) => section.name)).toEqual([
      'Group',
      'Header',
    ]);
  });

  it('counts a section as a group when it holds, or can hold, other sections', () => {
    const header = sheet.tables[0].sections.find((section) => section.role === 'Header');

    expect(group && isGroupSection(group)).toBe(true);
    expect(limits && isGroupSection(limits)).toBe(false);
    expect(header && isGroupSection(header)).toBe(false);
    expect(
      limits &&
        isGroupSection({
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

    expect(sectionTone(index, header?.rows[0].id ?? -1)).toBe('header');
    expect(sectionTone(index, group?.rows[0].id ?? -1)).toBe('group-0');
    expect(sectionTone(index, limits?.rows[0].id ?? -1)).toBe('plain');
    expect(sectionTone(index, -1)).toBe('plain');
  });

  it('picks out cells by id, ignoring ids the sheet does not have', () => {
    const first = limits?.rows[0].cells[0];

    expect([...cellsById(sheet, [first?.id ?? -1, -5])]).toEqual([first]);
    expect(cellsById(sheet, []).size).toBe(0);
  });
});
