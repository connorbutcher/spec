import { TemplateSection } from './template-section.model';

/** A section with where it sits in its template's tree. */
export interface SectionEntry {
  section: TemplateSection;
  /** 1 for a top-level section. */
  depth: number;
  /** The section's ancestors from the top level down, not including itself. */
  ancestors: TemplateSection[];
  /** The section's siblings, including itself, in display order. */
  siblings: TemplateSection[];
}
