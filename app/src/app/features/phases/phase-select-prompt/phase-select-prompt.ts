import { Component } from '@angular/core';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';

/** Shown on the right until a phase is picked from the tree. */
@Component({
  selector: 'app-phase-select-prompt',
  imports: [EmptyState],
  templateUrl: './phase-select-prompt.html',
  styleUrl: './phase-select-prompt.scss',
})
export class PhaseSelectPrompt {}
