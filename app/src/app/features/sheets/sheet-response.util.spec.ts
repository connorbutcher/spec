import { Sheet } from './models/sheet.model';
import { SheetResponseFixture } from './sheet-response.fixture';
import { SheetResponseUtil } from './sheet-response.util';
import { SheetStructureFixture } from './sheet-structure.fixture';

class SpecHelpers {
  public static sheetOf(): Sheet {
    const table = SheetStructureFixture.fixtureTable();
    const shared = SheetStructureFixture.fixtureCell(2);
    const twin = { ...SheetStructureFixture.fixtureCell(2), template: shared.template };
    table.sections = [
      SheetStructureFixture.fixtureSection(
        'Header',
        'Header',
        [SheetStructureFixture.fixtureRow([SheetStructureFixture.fixtureCell(1), shared])],
        [SheetStructureFixture.fixtureSection('Limits', 'Addable', [SheetStructureFixture.fixtureRow([twin])])],
      ),
    ];
    return {
      id: 5,
      publicId: 'sheet-5',
      phaseId: 1,
      sheetTypeId: 2,
      isLive: true,
      viewedVersionNumber: null,
      viewedAsOfUtc: null,
      latestVersionNumber: 1,
      myDraftCount: 0,
      otherDrafts: [],
      versions: [],
      tables: [table],
      availableTemplates: [],
      linkedSources: [],
    };
  }
}
describe('withTemplateCells', () => {
  it('gives every cell, child sections too, the template cell it names', () => {
    const sheet = SpecHelpers.sheetOf();

    expect(SheetResponseUtil.withTemplateCells(SheetResponseFixture.sheetResponse(sheet))).toEqual(sheet);
  });

  it('sends each template cell once, and cells built from one share its object', () => {
    const response = SheetResponseFixture.sheetResponse(SpecHelpers.sheetOf());
    const restored = SheetResponseUtil.withTemplateCells(response);
    const [section] = restored.tables[0].sections;

    expect(response.templateCells).toHaveLength(2);
    expect(section.sections[0].rows[0].cells[0].template).toBe(section.rows[0].cells[1].template);
  });

  it('fails loudly when a cell names a template cell the sheet does not list', () => {
    const response = { ...SheetResponseFixture.sheetResponse(SpecHelpers.sheetOf()), templateCells: [] };

    expect(() => SheetResponseUtil.withTemplateCells(response)).toThrow(/no template cell/);
  });
});
