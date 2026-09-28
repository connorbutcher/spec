import { Component, input } from '@angular/core';
import { Phase } from '../../../core/models/phase.model';

/** The phase's code and description, and where it sits in the tree. */
@Component({
  selector: 'app-phase-detail-header',
  template: `
    <header class="detail-header">
      <div class="title-row">
        <h1>{{ phase().code }}</h1>
        <span class="placement">
          @if (parent(); as parent) {
            Under {{ parent.code }}
          } @else {
            Top-level phase
          }
        </span>
      </div>
      @if (phase().description) {
        <p class="description">{{ phase().description }}</p>
      }
    </header>
  `,
  styleUrl: './phase-detail-header.component.scss',
})
export class PhaseDetailHeaderComponent {
  public readonly phase = input.required<Phase>();
  public readonly parent = input<Phase>();
}
