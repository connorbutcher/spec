/**
 * A kind of column block that can be added to a horizontal table, with how many copies there are now and
 * the template's limits. A null maximum means no limit.
 */
export interface AddableColumnBlock {
  templateColumnBlockId: number;
  name: string;
  count: number;
  minInstances: number;
  maxInstances: number | null;
  canAdd: boolean;
}
