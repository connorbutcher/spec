import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { LabelledTextSlotsEditor } from '../labelled-text-slots-editor/labelled-text-slots-editor';
import { LabelledTextSlot } from '../models/labelled-text-slot.model';

/**
 * The "Values" field of a labelled text cell type or template cell: the slot editor and, on a template
 * cell that has its own slots, a button that goes back to the cell type's.
 */
@Component({
  selector: 'app-labelled-text-slots-field',
  imports: [ButtonModule, LabelledTextSlotsEditor],
  templateUrl: './labelled-text-slots-field.html',
  styleUrl: './labelled-text-slots-field.scss',
  host: {
    '[class.overridden]': 'overridden()',
  },
})
export class LabelledTextSlotsField {
  public readonly slots = input.required<readonly LabelledTextSlot[]>();
  public readonly disabled = input(false);
  /** True on a template cell whose slots differ from its cell type's. */
  public readonly overridden = input(false);

  public readonly changed = output<LabelledTextSlot[]>();
  public readonly restored = output<void>();
}
