import { Component, input } from '@angular/core';

/** A centred icon, title and message for empty, loading-failed or not-yet-built views. */
@Component({
  selector: 'app-empty-state',
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.scss',
})
export class EmptyState {
  public readonly icon = input('pi-inbox');
  public readonly title = input.required<string>();
  public readonly message = input<string>();
}
