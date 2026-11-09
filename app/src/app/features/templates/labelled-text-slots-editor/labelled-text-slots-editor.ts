import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { LabelledTextSlotsUtil } from '../labelled-text-slots.util';
import { LabelledTextSlot } from '../models/labelled-text-slot.model';

/**
 * Edits the values a labelled text cell holds: relabel, reorder, remove and add. Each slot keeps the key
 * it was added with, so relabelling never loses what a sheet typed into it. Emits the full new list each
 * time. Used for a cell type's defaults, a template cell's override and one cell on a sheet.
 */
@Component({
  selector: 'app-labelled-text-slots-editor',
  imports: [ButtonModule, InputGroupAddonModule, InputGroupModule, InputTextModule, TooltipModule],
  templateUrl: './labelled-text-slots-editor.html',
  styleUrl: './labelled-text-slots-editor.scss',
})
export class LabelledTextSlotsEditor {
  public readonly slots = input.required<readonly LabelledTextSlot[]>();
  public readonly disabled = input(false);

  public readonly changed = output<LabelledTextSlot[]>();

  public readonly longestLabel = LabelledTextSlotsUtil.longestLabel;

  public isFull(): boolean {
    return this.slots().length >= LabelledTextSlotsUtil.mostSlots;
  }

  public relabel(index: number, input: HTMLInputElement): void {
    const label = input.value.trim();
    const slots = [...this.slots()];
    if (label !== slots[index].label) {
      slots[index] = { ...slots[index], label };
      this.changed.emit(slots);
    } else {
      input.value = slots[index].label;
    }
  }

  public move(index: number, offset: number): void {
    const slots = [...this.slots()];
    const target = index + offset;
    [slots[index], slots[target]] = [slots[target], slots[index]];
    this.changed.emit(slots);
  }

  public remove(index: number): void {
    this.changed.emit(this.slots().filter((_, position) => position !== index));
  }

  public add(input: HTMLInputElement): void {
    if (this.isFull()) {
      return;
    }
    const label = input.value.trim();
    const slots = this.slots();
    this.changed.emit([...slots, { key: LabelledTextSlotsUtil.keyFor(label, slots), label }]);
    input.value = '';
  }
}
