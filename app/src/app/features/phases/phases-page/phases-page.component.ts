import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PhaseTreePanelComponent } from '../phase-tree/phase-tree-panel.component';
import { PhasesStore } from '../phases.store';

/** The phases screen: the phase tree on the left and the selected phase on the right. */
@Component({
  selector: 'app-phases-page',
  imports: [RouterOutlet, PhaseTreePanelComponent],
  providers: [PhasesStore],
  template: `
    <div class="phases-page">
      <app-phase-tree-panel />
      <section class="detail" aria-live="polite">
        <router-outlet />
      </section>
    </div>
  `,
  styleUrl: './phases-page.component.scss',
})
export class PhasesPageComponent {
  // Instantiated here so the store (and its requests) starts with the page, not the first child.
  private readonly store = inject(PhasesStore);
}
