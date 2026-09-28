import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PhaseTreePanel } from '../phase-tree-panel/phase-tree-panel';
import { PhasesStore } from '../phases.store';

/** The phases screen: the phase tree on the left and the selected phase on the right. */
@Component({
  selector: 'app-phases-page',
  imports: [RouterOutlet, PhaseTreePanel],
  providers: [PhasesStore],
  templateUrl: './phases-page.html',
  styleUrl: './phases-page.scss',
})
export class PhasesPage {
  // Instantiated here so the store (and its requests) starts with the page, not the first child.
  private readonly store = inject(PhasesStore);
}
