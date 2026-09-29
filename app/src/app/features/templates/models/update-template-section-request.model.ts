import { SectionRole } from './section-role';

/** The body for changing a section. Every field is sent each time. */
export interface UpdateTemplateSectionRequest {
  name: string;
  role: SectionRole;
  minInstances: number;
  maxInstances: number | null;
  initialInstances: number;
}
