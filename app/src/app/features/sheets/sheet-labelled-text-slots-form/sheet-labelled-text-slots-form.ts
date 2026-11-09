import { Component, computed, input, linkedSignal, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogActions } from '../../../shared/components/dialog-actions/dialog-actions';
import { LabelledTextSlotsEditor } from '../../templates/labelled-text-slots-editor/labelled-text-slots-editor';
import { LabelledTextSlotsUtil } from '../../templates/labelled-text-slots.util';
import { LabelledTextSlot } from '../../templates/models/labelled-text-slot.model';
import { LabelledTextInstanceSettings } from '../models/labelled-text-instance-settings.model';

/**
 * The values one labelled text cell has on this sheet: relabel, reorder, add or remove them for this cell
 * alone. Nothing is saved until Apply; "Use template's" goes back to the template's values. A value whose
 * slot is removed is cleared.
 */
@Component({
  selector: 'app-sheet-labelled-text-slots-form',
  imports: [ButtonModule, DialogActions, LabelledTextSlotsEditor],
  templateUrl: './sheet-labelled-text-slots-form.html',
  styleUrl: './sheet-labelled-text-slots-form.scss',
})
export class SheetLabelledTextSlotsForm {
  /** The values the cell's template gives it. */
  public readonly templateSlots = input.required<readonly LabelledTextSlot[]>();
  /** The values chosen for this cell on the sheet, or null when it uses the template's. */
  public readonly chosen = input.required<readonly LabelledTextSlot[] | null>();

  /** The cell's own values, or null to go back to the template's. */
  public readonly applied = output<LabelledTextInstanceSettings | null>();
  public readonly cancelled = output<void>();

  public readonly draft = linkedSignal<readonly LabelledTextSlot[]>(() => this.chosen() ?? this.templateSlots());

  /** Whether Apply would change anything. */
  public readonly hasChanges = computed(() =>
    !LabelledTextSlotsUtil.same(this.draft(), this.chosen() ?? this.templateSlots()),
  );

  public apply(): void {
    const draft = this.draft();
    this.applied.emit(LabelledTextSlotsUtil.same(draft, this.templateSlots()) ? null : { kind: 'LabelledText', slots: draft });
  }
}
