/**
 * The body for changing a column block. Every field is sent each time; the counts must satisfy
 * fewest <= starts with <= most, and `maxInstances` null means no limit.
 */
export interface UpdateTemplateColumnBlockRequest {
  name: string;
  minInstances: number;
  maxInstances: number | null;
  initialInstances: number;
}
