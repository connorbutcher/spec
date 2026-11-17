import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.scss',
})
export class EmptyState {
  public readonly title = input.required<string>();
  public readonly icon = input('pi-inbox');
  public readonly message = input<string>();
}
