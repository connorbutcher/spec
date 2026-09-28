import { Component } from '@angular/core';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

/** Shown on the right until a phase is picked from the tree. */
@Component({
  selector: 'app-phase-select-prompt',
  imports: [EmptyStateComponent],
  template: `
    <div class="app-card">
      <app-empty-state
        icon="pi-sitemap"
        title="Select a phase"
        message="Pick a phase from the tree to see the PU Spec Sheet types available to it."
      />
    </div>
  `,
})
export class PhaseSelectPromptComponent {}
