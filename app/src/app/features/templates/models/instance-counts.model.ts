/** How many copies of a section or column block a sheet table may hold. A null maximum means no limit. */
export type InstanceCounts = {
  minInstances: number;
  maxInstances: number | null;
  initialInstances: number;
}
