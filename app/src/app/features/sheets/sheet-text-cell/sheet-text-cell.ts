import { Component, computed, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TextareaModule } from 'primeng/textarea';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';
import { CellValueUtil } from '../values/cell-value.util';

/**
 * The editor for a text cell: a PrimeNG text area that fills the whole cell and grows with its text, so
 * a long value wraps onto more lines rather than being cut off. A single-line cell takes no line breaks:
 * Enter saves it instead. The value is saved when focus leaves it too; Escape puts back the saved value.
 * Nothing is emitted if the text hasn't changed.
 */
@Component({
  selector: 'app-sheet-text-cell',
  imports: [FormsModule, TextareaModule],
  templateUrl: './sheet-text-cell.html',
  styleUrl: './sheet-text-cell.scss',
})

export class SheetTextCell {
  public readonly cell = input.required<SheetCell>();
  public readonly configuration = input.required<CellConfiguration>();
  /** The cell's caption, shown as a hint while the cell is empty. */
  public readonly label = input('');

  /** The user moved into the cell to edit it. */
  public readonly started = output<void>();
  public readonly changed = output<CellValueRequest>();

  public readonly text = linkedSignal(() => this.cell().textValue ?? '');

  public readonly settings = computed(() => {
    const configuration = this.configuration();
    return configuration.kind === 'Text' ? configuration : null;
  });

  /** Enter starts a new line in a multi-line cell, and saves a single-line one. */
  public enter(event: Event): void {
    if (this.settings()?.multiline) {
      return;
    }
    event.preventDefault();
    this.commit();
  }

  public commit(): void {
    // Spaces around the text aren't a change: it's shown as saved if that's all that differs. Line
    // breaks pasted into a single-line cell become spaces.
    const typed = this.settings()?.multiline ? this.text() : this.text().replace(/\s*[\r\n]+\s*/g, ' ');
    const value = typed.trim();
    if (value === (this.cell().textValue ?? '').trim()) {
      this.revert();
      return;
    }
    this.changed.emit(CellValueUtil.valueRequest(this.cell(), 'Text', value === '' ? null : value));
  }

  public revert(): void {
    this.text.set(this.cell().textValue ?? '');
  }
}
