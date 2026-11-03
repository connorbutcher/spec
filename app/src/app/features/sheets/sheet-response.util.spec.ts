import { Sheet } from './models/sheet.model';
import { sheetResponse } from './sheet-response.fixture';
import { withTemplateCells } from './sheet-response.util';
import { fixtureCell, fixtureRow, fixtureSection, fixtureTable } from './sheet-structure.fixture';

function sheetOf(): Sheet {
  const table = fixtureTable();
  const shared = fixtureCell(2);
  const twin = { ...fixtureCell(2), template: shared.template };
  table.sections = [
    fixtureSection(
      'Header',
      'Header',
      [fixtureRow([fixtureCell(1), shared])],
      [fixtureSection('Limits', 'Addable', [fixtureRow([twin])])],
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

describe('withTemplateCells', () => {
  it('gives every cell, child sections too, the template cell it names', () => {
    const sheet = sheetOf();

    expect(withTemplateCells(sheetResponse(sheet))).toEqual(sheet);
  });

  it('sends each template cell once, and cells built from one share its object', () => {
    const response = sheetResponse(sheetOf());
    const restored = withTemplateCells(response);
    const [section] = restored.tables[0].sections;

    expect(response.templateCells).toHaveLength(2);
    expect(section.sections[0].rows[0].cells[0].template).toBe(section.rows[0].cells[1].template);
  });

  it('fails loudly when a cell names a template cell the sheet does not list', () => {
    const response = { ...sheetResponse(sheetOf()), templateCells: [] };

    expect(() => withTemplateCells(response)).toThrow(/no template cell/);
  });
});
