import { Component, input } from '@angular/core';
import { Phase } from '../../../core/models/phase.model';

/** The phase's code and description, and where it sits in the tree. */
@Component({
  selector: 'app-phase-detail-header',
  templateUrl: './phase-detail-header.html',
  styleUrl: './phase-detail-header.scss',
})
export class PhaseDetailHeader {
  public readonly phase = input.required<Phase>();
  public readonly parent = input<Phase>();
}
