/**
 * A kind of section that can be added under a table or another section, with how many copies there are
 * now and the template's limits. A null maximum means no limit.
 */
export interface AddableSection {
  templateSectionId: number;
  name: string;
  count: number;
  minInstances: number;
  maxInstances: number | null;
  canAdd: boolean;
}
