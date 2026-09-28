import { Component, input } from '@angular/core';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';

/** Placeholder for sections that aren't built yet. Title and icon come from the route's data. */
@Component({
  selector: 'app-coming-soon-page',
  imports: [EmptyState],
  templateUrl: './coming-soon-page.html',
  styleUrl: './coming-soon-page.scss',
})
export class ComingSoonPage {
  public readonly title = input.required<string>();
  public readonly icon = input('pi-wrench');
}
