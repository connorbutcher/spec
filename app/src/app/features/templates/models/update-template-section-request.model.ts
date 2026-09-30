/**
 * The body for changing a section. Every field is sent each time. The counts only apply to an addable
 * section: the header is always exactly one copy. `maxInstances` null means no limit.
 */
export interface UpdateTemplateSectionRequest {
  name: string;
  minInstances: number;
  maxInstances: number | null;
  initialInstances: number;
}
