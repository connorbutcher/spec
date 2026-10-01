/**
 * Which moment of a sheet to look at: the live state (nothing set), the state when a version was
 * published, or the state at a date and time (an ISO string).
 */
export interface SheetView {
  version?: number;
  asOf?: string;
}
