import { Phase } from './phase.model';

/** A phase with its child phases attached, for rendering the phase tree. */
export interface PhaseTreeNode {
  phase: Phase;
  children: PhaseTreeNode[];
}
