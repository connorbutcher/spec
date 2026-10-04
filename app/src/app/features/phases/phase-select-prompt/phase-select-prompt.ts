import { Component } from '@angular/core';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';

/** Shown until a phase is picked from the phase tree in the side nav. */
@Component({
  selector: 'app-phase-select-prompt',
  imports: [EmptyState],
  templateUrl: './phase-select-prompt.html',
  styleUrl: './phase-select-prompt.scss',
})
export class PhaseSelectPrompt {}
