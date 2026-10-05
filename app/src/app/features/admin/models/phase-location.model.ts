/** Where a phase sits in the tree: its parent (null at the top level) and its 1-based place among its siblings. */
export interface PhaseLocation {
  parentPhaseId: number | null;
  position: number;
}
