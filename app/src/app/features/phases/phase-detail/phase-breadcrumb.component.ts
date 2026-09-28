import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Phase } from '../../../core/models/phase.model';

/** Phases › V6 › 01-A2 — the path from the top of the tree to the current phase. */
@Component({
  selector: 'app-phase-breadcrumb',
  imports: [RouterLink],
  template: `
    <nav class="breadcrumb" aria-label="Breadcrumb">
      <ol>
        <li><a routerLink="/phases">Phases</a></li>
        @for (ancestor of ancestors(); track ancestor.id) {
          <li>
            <a [routerLink]="['/phases', ancestor.id]">{{ ancestor.code }}</a>
          </li>
        }
        <li>
          <span aria-current="page">{{ current().code }}</span>
        </li>
      </ol>
    </nav>
  `,
  styleUrl: './phase-breadcrumb.component.scss',
})
export class PhaseBreadcrumbComponent {
  public readonly ancestors = input.required<Phase[]>();
  public readonly current = input.required<Phase>();
}
