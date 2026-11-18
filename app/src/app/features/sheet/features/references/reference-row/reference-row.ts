import { Component, input } from '@angular/core';
import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet';

@Component({
  selector: 'app-reference-row',
  templateUrl: './reference-row.html',
  styleUrl: './reference-row.scss',
})
export class ReferenceRow {
  public readonly systemReference = input.required<SheetExternalReference>();
  public readonly highlightedReferenceId = input<string>();
}
