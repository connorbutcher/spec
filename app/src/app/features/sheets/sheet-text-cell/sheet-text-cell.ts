import { Component, computed, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';
import { valueRequest } from '../cell-value.util';

/**
 * The editor for a text cell: a PrimeNG text box (or text area, if the cell is multi-line) that fills
 * the whole cell. The value is saved when focus leaves it, or on Enter in a single-line box; Escape puts
 * back the saved value. Nothing is emitted if the text hasn't changed.
 */
@Component({
  selector: 'app-sheet-text-cell',
  imports: [FormsModule, InputTextModule, TextareaModule],
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

  public commit(): void {
    // Spaces around the text aren't a change: it's shown as saved if that's all that differs.
    const value = this.text().trim();
    if (value === (this.cell().textValue ?? '').trim()) {
      this.revert();
      return;
    }
    this.changed.emit(valueRequest(this.cell(), 'Text', value === '' ? null : value));
  }

  public revert(): void {
    this.text.set(this.cell().textValue ?? '');
  }
}
