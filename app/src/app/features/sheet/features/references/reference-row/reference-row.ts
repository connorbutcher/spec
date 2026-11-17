import { Component, input } from '@angular/core';
import { SpecSheetExternalReference } from '../../../../../core/models/domain/references/spec-sheet-external-reference';

@Component({
  selector: 'app-reference-row',
  templateUrl: './reference-row.html',
  styleUrl: './reference-row.scss',
})
export class ReferenceRow {
  public readonly systemReference =
    input.required<SpecSheetExternalReference>();
  public readonly highlightedReferenceId = input<string>();
}
