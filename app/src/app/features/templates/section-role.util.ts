import { TemplateSection } from './models/template-section.model';

/** Whether a section is the table's header. */
export function isHeader(section: TemplateSection): boolean {
  return section.role === 'Header';
}

/** What to call a section: "Header", "Section" at the top level, or "Sub-section" inside another. */
export function sectionNoun(section: TemplateSection): string {
  if (isHeader(section)) {
    return 'Header';
  }
  return section.parentSectionId === null ? 'Section' : 'Sub-section';
}

/**
 * Plain wording for how many copies a sheet table can have of a section, e.g. "Added on the sheet: any
 * number, starts with none" or "Added on the sheet: 1 to 5, starts with 1".
 */
export function describeInstances(section: TemplateSection): string {
  if (isHeader(section)) {
    return 'Always one';
  }
  const { minInstances, maxInstances, initialInstances } = section;
  let range = `${minInstances} to ${maxInstances}`;
  if (maxInstances === null) {
    range = minInstances > 0 ? `at least ${minInstances}` : 'any number';
  }
  const start = initialInstances > 0 ? `starts with ${initialInstances}` : 'starts with none';
  return `Added on the sheet: ${range}, ${start}`;
}

/** A short tag for the structure tree: "header", "×0+", "×1–5". */
export function instanceTag(section: TemplateSection): string {
  if (isHeader(section)) {
    return 'header';
  }
  const { minInstances, maxInstances } = section;
  return maxInstances === null ? `×${minInstances}+` : `×${minInstances}–${maxInstances}`;
}
