import { Component, input, output } from '@angular/core';
import { CellTypeOption } from '../models/cell-type-option.model';

/** Edits a dropdown's choices: rename, reorder, remove and add. Emits the full new list each time. */
@Component({
  selector: 'app-cell-type-options-editor',
  templateUrl: './cell-type-options-editor.html',
  styleUrl: './cell-type-options-editor.scss',
})
export class CellTypeOptionsEditor {
  public readonly options = input.required<CellTypeOption[]>();
  public readonly disabled = input(false);

  public readonly changed = output<string[]>();

  public rename(index: number, input: HTMLInputElement): void {
    const value = input.value.trim();
    const current = this.values();
    if (value && value !== current[index]) {
      current[index] = value;
      this.changed.emit(current);
    } else {
      input.value = current[index];
    }
  }

  public move(index: number, offset: number): void {
    const values = this.values();
    const target = index + offset;
    [values[index], values[target]] = [values[target], values[index]];
    this.changed.emit(values);
  }

  public remove(index: number): void {
    this.changed.emit(this.values().filter((_, position) => position !== index));
  }

  public add(input: HTMLInputElement): void {
    const value = input.value.trim();
    if (value) {
      this.changed.emit([...this.values(), value]);
      input.value = '';
    }
  }

  private values(): string[] {
    return this.options().map((option) => option.value);
  }
}
