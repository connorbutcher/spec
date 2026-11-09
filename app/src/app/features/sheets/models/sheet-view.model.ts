/**
 * Which moment of a sheet to look at: the live state (nothing set), the state when a version was
 * published, the state at a date and time (an ISO string), or the live state with a submission's changes
 * waiting for approval on top, for its approvers to check.
 */
export type SheetView = {
  version?: number;
  asOf?: string;
  submission?: number;
}
