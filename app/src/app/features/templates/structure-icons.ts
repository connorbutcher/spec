import { PanelRef } from './models/panel-ref';
import { StructureLevel } from './models/structure-level';
import { TemplateIndex } from './models/template-index.model';
import { TemplateSection } from './models/template-section.model';
import { SectionRoleUtil } from './section-role.util';

const ICONS: Readonly<Record<StructureLevel, string>> = {
  table: 'pi-table',
  header: 'pi-window-maximize',
  section: 'pi-folder',
  subSection: 'pi-sitemap',
  row: 'pi-bars',
  cell: 'pi-stop',
  columnBlock: 'pi-pause',
  columnBlocks: 'pi-pause',
  cellTypes: 'pi-th-large',
  cellType: 'pi-tag',
};

const NOUNS: Readonly<Record<StructureLevel, string>> = {
  table: 'Table',
  header: 'Header',
  section: 'Section',
  subSection: 'Sub-section',
  row: 'Row',
  cell: 'Cell',
  columnBlock: 'Column block',
  columnBlocks: 'Column blocks',
  cellTypes: 'Cell types',
  cellType: 'Cell type',
};

/**
 * One icon and one name per level of a table's structure, so the tree, the breadcrumb, the menus and
 * the panel title all agree. The level's colour comes from the `level-<level>` class (see `_structure-levels.scss`).
 */
export class StructureIcons {
  public static icon(level: StructureLevel): string {
    return ICONS[level];
  }

  public static noun(level: StructureLevel): string {
    return NOUNS[level];
  }

  /** The CSS class that colours a level's icon. */
  public static levelClass(level: StructureLevel): string {
    return `level-${level}`;
  }

  public static sectionLevel(section: TemplateSection): StructureLevel {
    if (SectionRoleUtil.isHeader(section)) {
      return 'header';
    }
    return section.parentSectionId === null ? 'section' : 'subSection';
  }

  /** The level of what a panel shows. A section's level depends on where it sits. */
  public static levelOf(ref: PanelRef, index: TemplateIndex): StructureLevel {
    switch (ref.kind) {
      case 'template':
        return 'table';
      case 'section': {
        const section = index.sections.get(ref.id)?.section;
        return section ? StructureIcons.sectionLevel(section) : 'section';
      }
      default:
        return ref.kind;
    }
  }
}
