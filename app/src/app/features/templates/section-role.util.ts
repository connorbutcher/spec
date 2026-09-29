import { FixedPresence } from './models/fixed-presence';
import { SectionRole } from './models/section-role';
import { TemplateSection } from './models/template-section.model';
import { UpdateTemplateSectionRequest } from './models/update-template-section-request.model';

type InstanceCounts = Pick<
  UpdateTemplateSectionRequest,
  'role' | 'minInstances' | 'maxInstances' | 'initialInstances'
>;

/** The instance counts each fixed-section choice stands for. */
const FIXED_COUNTS: Readonly<Record<FixedPresence, InstanceCounts>> = {
  always: { role: 'Fixed', minInstances: 1, maxInstances: 1, initialInstances: 1 },
  default: { role: 'Fixed', minInstances: 0, maxInstances: 1, initialInstances: 1 },
  optional: { role: 'Fixed', minInstances: 0, maxInstances: 1, initialInstances: 0 },
};

/** Which fixed-section choice a section's counts match. */
export function fixedPresence(section: TemplateSection): FixedPresence {
  if (section.minInstances >= 1) {
    return 'always';
  }
  return section.initialInstances >= 1 ? 'default' : 'optional';
}

export function fixedCounts(presence: FixedPresence): InstanceCounts {
  return FIXED_COUNTS[presence];
}

/**
 * The counts after switching a section to `role`: a repeating section keeps its minimum and starting
 * copies with no maximum; a fixed one keeps whichever single-block choice is closest.
 */
export function countsForRole(section: TemplateSection, role: SectionRole): InstanceCounts {
  if (role === 'Repeating') {
    return {
      role,
      minInstances: section.minInstances,
      maxInstances: null,
      initialInstances: Math.max(section.initialInstances, section.minInstances),
    };
  }
  if (section.minInstances >= 1) {
    return FIXED_COUNTS.always;
  }
  return section.initialInstances >= 1 ? FIXED_COUNTS.default : FIXED_COUNTS.optional;
}

/** A short description of the counts, e.g. "1 to 5, starts with 1" or "always included". */
export function describeInstances(section: TemplateSection): string {
  if (section.role === 'Fixed') {
    return {
      always: 'Always included',
      default: 'Included, removable',
      optional: 'Optional',
    }[fixedPresence(section)];
  }
  const max = section.maxInstances === null ? 'any number' : `up to ${section.maxInstances}`;
  return `Repeats: ${section.minInstances} min, ${max}, starts with ${section.initialInstances}`;
}
