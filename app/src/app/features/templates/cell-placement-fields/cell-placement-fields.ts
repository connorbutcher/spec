import { Component, input, output } from '@angular/core';
import { CellPlacement } from '../models/cell-placement.model';
import { NumberField } from '../number-field/number-field';

/** The "Placement" fields of a cell: its column, column span and row span. Blank values aren't saved. */
@Component({
  selector: 'app-cell-placement-fields',
  imports: [NumberField],
  templateUrl: './cell-placement-fields.html',
  styleUrl: './cell-placement-fields.scss',
})
export class CellPlacementFields {
  public readonly placement = input.required<CellPlacement>();
  public readonly disabled = input(false);

  public readonly changed = output<Partial<CellPlacement>>();

  public set(field: keyof CellPlacement, value: number | null): void {
    if (value !== null) {
      this.changed.emit({ [field]: value });
    }
  }
}
