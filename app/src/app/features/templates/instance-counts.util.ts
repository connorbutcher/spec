import { InstanceCounts } from './models/instance-counts.model';
import { InstanceField } from './models/instance-field';

/** Sets one of the counts, nudging the others so fewest <= starts with <= most still holds. */
export function withCount(
  current: InstanceCounts,
  field: InstanceField,
  value: number | null,
): InstanceCounts {
  const counts: InstanceCounts = {
    minInstances: current.minInstances,
    maxInstances: current.maxInstances,
    initialInstances: current.initialInstances,
    [field]: field === 'maxInstances' ? value : (value ?? 0),
  };
  if (field === 'minInstances') {
    counts.initialInstances = Math.max(counts.initialInstances, counts.minInstances);
  }
  if (counts.maxInstances !== null) {
    counts.initialInstances = Math.min(counts.initialInstances, counts.maxInstances);
    counts.minInstances = Math.min(counts.minInstances, counts.initialInstances);
  }
  return counts;
}

/** A short tag for how many copies a sheet table can have: "×0+", "×1–5". */
export function copiesTag(counts: InstanceCounts): string {
  const { minInstances, maxInstances } = counts;
  return maxInstances === null ? `×${minInstances}+` : `×${minInstances}–${maxInstances}`;
}
