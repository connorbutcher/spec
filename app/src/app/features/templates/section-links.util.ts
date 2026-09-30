import { PanelLinkItem } from './models/panel-link-item.model';
import { TemplateSection } from './models/template-section.model';

/** The icon for a section: a header, an addable section, or a sub-section. */
export function sectionIcon(section: TemplateSection): string {
  if (section.role === 'Header') {
    return 'pi-table';
  }
  return section.parentSectionId === null ? 'pi-clone' : 'pi-sitemap';
}

/** Sections as panel links, with what each one holds. */
export function sectionLinks(sections: TemplateSection[]): PanelLinkItem[] {
  return sections.map((section) => ({
    ref: { kind: 'section', id: section.id },
    label: section.name,
    icon: sectionIcon(section),
    meta: sectionContents(section),
  }));
}

/** "2 rows · 1 sub-section", or "empty". */
export function sectionContents(section: TemplateSection): string {
  const parts = [
    section.rows.length > 0 ? plural(section.rows.length, 'row') : '',
    section.sections.length > 0 ? plural(section.sections.length, 'sub-section') : '',
  ].filter(Boolean);
  return parts.length > 0 ? parts.join(' · ') : 'empty';
}

export function plural(count: number, noun: string): string {
  return `${count} ${noun}${count === 1 ? '' : 's'}`;
}
