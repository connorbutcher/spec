import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Phase } from '../../../core/models/phase.model';

/** Links to the phases directly under the current one, with how many sheet types each has. */
@Component({
  selector: 'app-child-phase-list',
  imports: [RouterLink],
  template: `
    <ul class="child-phases">
      @for (child of phases(); track child.id) {
        <li>
          <a [routerLink]="['/phases', child.id]">
            <span class="code">{{ child.code }}</span>
            <span class="meta">{{ child.sheetTypeIds.length }} sheet types</span>
            <i class="pi pi-angle-right" aria-hidden="true"></i>
          </a>
        </li>
      }
    </ul>
  `,
  styleUrl: './child-phase-list.component.scss',
})
export class ChildPhaseListComponent {
  public readonly phases = input.required<Phase[]>();
}
