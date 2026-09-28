import { Component, input } from '@angular/core';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

/** Placeholder for sections that aren't built yet. Title and icon come from the route's data. */
@Component({
  selector: 'app-coming-soon-page',
  imports: [EmptyStateComponent],
  template: `
    <section class="app-card">
      <app-empty-state
        [icon]="icon()"
        [title]="title()"
        message="This section hasn't been built yet."
      />
    </section>
  `,
})
export class ComingSoonPageComponent {
  public readonly title = input.required<string>();
  public readonly icon = input('pi-wrench');
}
