/**
 * Whether the viewer could ask to take over a row right now. When they couldn't, `reason` says why in
 * words to show them, e.g. the row has unpublished changes.
 */
export interface RowTakeoverAvailability {
  isAvailable: boolean;
  reason: string | null;
}
