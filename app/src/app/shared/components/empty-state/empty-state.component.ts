import { Component, input } from '@angular/core';

/** A centred icon, title and message for empty, loading-failed or not-yet-built views. */
@Component({
  selector: 'app-empty-state',
  template: `
    <div class="empty-state">
      <i class="pi" [class]="icon()" aria-hidden="true"></i>
      <h2>{{ title() }}</h2>
      @if (message()) {
        <p>{{ message() }}</p>
      }
      <ng-content />
    </div>
  `,
  styleUrl: './empty-state.component.scss',
})
export class EmptyStateComponent {
  public readonly icon = input('pi-inbox');
  public readonly title = input.required<string>();
  public readonly message = input<string>();
}
