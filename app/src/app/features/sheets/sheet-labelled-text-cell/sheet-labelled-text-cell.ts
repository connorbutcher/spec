import { Component, computed, ElementRef, inject, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { LabelledTextSlotsUtil } from '../../templates/labelled-text-slots.util';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';
import { LabelledTextValueUtil } from '../values/labelled-text-value.util';
import { CellValueUtil } from '../values/cell-value.util';

/**
 * The editor for a labelled text cell: a PrimeNG text box for each value, with its label beside it. The
 * values are saved together when focus leaves the cell, or on Enter in the last box; Enter in any other
 * box moves to the next one. Escape puts back the saved values. Nothing is emitted if nothing changed.
 */
@Component({
  selector: 'app-sheet-labelled-text-cell',
  imports: [FormsModule, InputTextModule],
  templateUrl: './sheet-labelled-text-cell.html',
  styleUrl: './sheet-labelled-text-cell.scss',
  host: {
    '(focusin)': 'started.emit()',
    '(focusout)': 'leave($event)',
  },
})
export class SheetLabelledTextCell {
  public readonly cell = input.required<SheetCell>();
  public readonly configuration = input.required<CellConfiguration>();
  /** The cell's caption, read out with each box. */
  public readonly label = input('');

  /** The user moved into the cell to edit it. */
  public readonly started = output<void>();
  public readonly changed = output<CellValueRequest>();

  public readonly slots = computed(() => LabelledTextValueUtil.slotsOf(this.cell(), this.configuration()));

  public readonly values = linkedSignal<Record<string, string>>(() => ({ ...(CellValueUtil.slots(this.cell()) ?? {}) }));

  public readonly labelColor = computed(() => LabelledTextSlotsUtil.labelColor(this.configuration()));

  public readonly maxLength = computed(() => {
    const configuration = this.configuration();
    return configuration.kind === 'LabelledText' ? (configuration.maxLength ?? null) : null;
  });

  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);

  public set(key: string, text: string): void {
    this.values.update((values) => ({ ...values, [key]: text }));
  }

  /** Enter moves to the next box, and saves from the last one. */
  public enter(event: Event, index: number): void {
    event.preventDefault();
    const boxes = this.element.nativeElement.querySelectorAll('input');
    if (index + 1 < boxes.length) {
      boxes[index + 1].focus();
      return;
    }
    this.commit();
  }

  /** Focus moving to another box of the same cell isn't leaving it. */
  public leave(event: FocusEvent): void {
    const next = event.relatedTarget;
    if (!(next instanceof Node) || !this.element.nativeElement.contains(next)) {
      this.commit();
    }
  }

  public commit(): void {
    const values = LabelledTextValueUtil.cleaned(this.slots(), this.values());
    const saved = LabelledTextValueUtil.cleaned(this.slots(), CellValueUtil.slots(this.cell()) ?? {});
    if (LabelledTextValueUtil.same(values, saved)) {
      this.revert();
      return;
    }
    this.changed.emit({ sheetCellId: this.cell().id, value: values });
  }

  public revert(): void {
    this.values.set({ ...(CellValueUtil.slots(this.cell()) ?? {}) });
  }

  public ariaLabel(slotLabel: string, index: number): string {
    const name = this.label() || 'Value';
    return slotLabel ? `${name}, ${slotLabel}` : `${name} ${index + 1}`;
  }
}
