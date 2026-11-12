import { Component, computed, input } from '@angular/core';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { LabelledTextSlotsUtil } from '../../templates/labelled-text-slots.util';
import { SheetCell } from '../models/sheet-cell.model';
import { LabelledTextValueUtil } from '../values/labelled-text-value.util';
import { CellValueUtil } from '../values/cell-value.util';

/**
 * A labelled text cell as plain content: each value on its own line with its label after it, in the
 * label colour the template gives. The caption stands in when the cell holds nothing.
 */
@Component({
  selector: 'app-sheet-labelled-text-value',
  templateUrl: './sheet-labelled-text-value.html',
  styleUrl: './sheet-labelled-text-value.scss',
})
export class SheetLabelledTextValue {
  public readonly cell = input.required<SheetCell>();
  public readonly configuration = input.required<CellConfiguration | null>();
  /** Shown in place of values the cell doesn't have. */
  public readonly hint = input('');

  public readonly slots = computed(() => LabelledTextValueUtil.slotsOf(this.cell(), this.configuration()));

  public readonly values = computed<Readonly<Record<string, string>>>(() => CellValueUtil.slots(this.cell()) ?? {});

  public readonly isEmpty = computed(() => this.slots().every((slot) => !this.values()[slot.key]));

  public readonly labelColor = computed(() => LabelledTextSlotsUtil.labelColor(this.configuration()));
}
