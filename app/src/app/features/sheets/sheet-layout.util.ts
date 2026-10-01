import { TableTemplate } from '../templates/models/table-template.model';
import { TemplateLayout } from '../templates/models/template-layout.model';
import { TemplateSection } from '../templates/models/template-section.model';
import { layoutTemplate } from '../templates/template-layout.util';
import { SheetSection } from './models/sheet-section.model';
import { SheetTable } from './models/sheet-table.model';

/**
 * Lays a sheet table out with the template layout util, so it looks exactly like the template. The
 * table's section copies, rows and cells are handed over as a template, with each item's id being the
 * sheet item's own, so what the layout places can be matched back to the sheet.
 */
export function layoutSheetTable(table: SheetTable): TemplateLayout {
  const template: TableTemplate = {
    id: table.tableTemplateId,
    sheetTypeId: 0,
    name: table.templateName,
    displayOrder: table.displayOrder,
    versionId: 0,
    versionNumber: table.templateVersionNumber,
    orientation: table.orientation,
    isEditable: false,
    versions: [],
    sections: table.sections.map((section) => toTemplateSection(section, null)),
  };
  return layoutTemplate(template);
}

function toTemplateSection(section: SheetSection, parentId: number | null): TemplateSection {
  return {
    id: section.id,
    parentSectionId: parentId,
    name: section.name,
    displayOrder: section.displayOrder,
    role: section.role,
    minInstances: section.minInstances,
    maxInstances: section.maxInstances,
    initialInstances: section.initialInstances,
    sections: section.sections.map((child) => toTemplateSection(child, section.id)),
    rows: section.rows.map((row) => ({
      id: row.id,
      displayOrder: row.displayOrder,
      cells: row.cells.map((cell) => ({ ...cell.template, id: cell.id })),
    })),
  };
}
