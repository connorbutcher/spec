import { SheetCell } from '../models/sheet-cell.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetSection } from '../models/sheet-section.model';
import { SheetTable } from '../models/sheet-table.model';

let nextId = 1;

export class SheetStructureFixture {
  /** A cell at a column, spanning `columnSpan` columns. */
  public static fixtureCell(column: number, columnSpan = 1): SheetCell {
    const id = nextId++;
    return {
      id,
      publicId: `cell-${id}`,
      template: {
        id: 1000 + id,
        cellTypeId: 1,
        column,
        rowSpan: 1,
        columnSpan,
        caption: null,
        isRequired: false,
        configurationOverride: null,
        styleOverride: null,
        columnBlockId: null,
      },
      textValue: null,
      numberValue: null,
      dateValue: null,
      booleanValue: null,
      optionId: null,
      settings: null,
      sheetColumnBlockId: null,
      lastChange: null,
    };
  }

  public static fixtureRow(cells: SheetCell[]): SheetRow {
    const id = nextId++;
    return {
      id,
      publicId: `row-${id}`,
      templateRowId: 0,
      displayOrder: id,
      lock: null,
      isPending: false,
      canRemove: true,
      lastChange: null,
      cells,
    };
  }

  public static fixtureSection(name: string, role: 'Header' | 'Addable', rows: SheetRow[], sections: SheetSection[] = []): SheetSection {
    const id = nextId++;
    return {
      id,
      publicId: `section-${id}`,
      templateSectionId: 0,
      name,
      role,
      minInstances: 0,
      maxInstances: null,
      initialInstances: 0,
      displayOrder: id,
      lock: null,
      isPending: false,
      canRemove: role === 'Addable',
      rows,
      sections,
      addableSections: [],
      addableRows: [],
      lastChange: null,
    };
  }

  /** A vertical table: a header of five columns, then a group holding its own row and one sub-section. */
  public static fixtureTable(): SheetTable {
    const header = SheetStructureFixture.fixtureSection('Header', 'Header', [
      SheetStructureFixture.fixtureRow([1, 2, 3, 4, 5].map((column) => SheetStructureFixture.fixtureCell(column))),
    ]);
    const limits = SheetStructureFixture.fixtureSection('Limits', 'Addable', [
      SheetStructureFixture.fixtureRow([1, 2, 3, 4, 5].map((column) => SheetStructureFixture.fixtureCell(column))),
    ]);
    const group = SheetStructureFixture.fixtureSection('Group', 'Addable', [SheetStructureFixture.fixtureRow([SheetStructureFixture.fixtureCell(1, 5)])], [limits]);
    return {
      id: 1,
      publicId: 'table-1',
      tableTemplateId: 1,
      templateName: 'Limits table',
      templateVersionNumber: 1,
      orientation: 'Vertical',
      stickyColumnCount: 0,
      title: null,
      displayOrder: 1,
      lock: null,
      isPending: false,
      sections: [group, header],
      addableSections: [],
      columnBlocks: [],
      addableColumnBlocks: [],
      linkableColumns: [],
      linkableRows: [],

    };
  }
}
