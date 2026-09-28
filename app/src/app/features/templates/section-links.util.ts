import { PanelLinkItem } from './models/panel-link-item.model';
import { TemplateSection } from './models/template-section.model';

/** Sections as panel links, with what each one holds. */
export function sectionLinks(sections: TemplateSection[]): PanelLinkItem[] {
  return sections.map((section) => ({
    ref: { kind: 'section', id: section.id },
    label: section.name,
    icon: section.sections.length > 0 ? 'pi-folder' : 'pi-table',
    meta: sectionContents(section),
  }));
}

/** "3 sections", "2 rows" or "empty". */
export function sectionContents(section: TemplateSection): string {
  if (section.sections.length > 0) {
    return plural(section.sections.length, 'section');
  }
  return section.rows.length > 0 ? plural(section.rows.length, 'row') : 'empty';
}

export function plural(count: number, noun: string): string {
  return `${count} ${noun}${count === 1 ? '' : 's'}`;
}
