import { Component, input, signal } from '@angular/core';
import { SystemReferenceGroup } from '../models/system-reference-group';
import { ReferenceRow } from '../reference-row/reference-row';

@Component({
  selector: 'app-reference-group',
  imports: [ReferenceRow],
  templateUrl: './reference-group.html',
  styleUrl: './reference-group.scss',
})
export class ReferenceGroup {
  public readonly systemReferenceGroup = input.required<SystemReferenceGroup>();
  public readonly highlightedReferenceId = input<string>();
  public readonly isExpanded = signal<boolean>(true);

  public toggleExpanded(): void {
    this.isExpanded.set(!this.isExpanded());
  }
}
