import { Component, input } from '@angular/core';
import { SystemReference } from '../models/system-reference';
import { ReferenceGroup } from '../reference-group/reference-group';

@Component({
  selector: 'app-active-system-reference',
  imports: [ReferenceGroup],
  templateUrl: './active-system-reference.html',
  styleUrl: './active-system-reference.scss',
})
export class ActiveSystemReference {
  public readonly systemReference = input.required<SystemReference>();
  public readonly highlightedReferenceId = input<string>();
}
