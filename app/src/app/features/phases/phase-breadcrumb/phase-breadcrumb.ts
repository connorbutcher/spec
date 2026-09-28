import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Phase } from '../../../core/models/phase.model';

/** Phases › V6 › 01-A2 — the path from the top of the tree to the current phase. */
@Component({
  selector: 'app-phase-breadcrumb',
  imports: [RouterLink],
  templateUrl: './phase-breadcrumb.html',
  styleUrl: './phase-breadcrumb.scss',
})
export class PhaseBreadcrumb {
  public readonly ancestors = input.required<Phase[]>();
  public readonly current = input.required<Phase>();
}
