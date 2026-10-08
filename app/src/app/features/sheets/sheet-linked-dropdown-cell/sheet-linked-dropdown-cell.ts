import { Component, computed, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { valueRequest } from '../cell-value.util';
import { linkedHint, linkedOptions } from '../linked-dropdown.util';
import { CellValueRequest } from '../models/cell-value-request.model';
import { LinkedChoices } from '../models/linked-choices.model';
import { SheetCell } from '../models/sheet-cell.model';

/** A list this long gets a search box. */
const FILTER_FROM = 8;

/**
 * The editor for a linked dropdown cell: a PrimeNG select of the values in the column the cell is
 * pointed at. The choice is saved as soon as it's made, as its text, and can be cleared. Until the cell
 * has a column there is nothing to choose from, so the select is off and says why; the settings button
 * beside it (`SheetCellSettings`) is where the column is chosen.
 */
@Component({
  selector: 'app-sheet-linked-dropdown-cell',
  imports: [FormsModule, SelectModule],
  templateUrl: './sheet-linked-dropdown-cell.html',
  styleUrl: './sheet-linked-dropdown-cell.scss',
})
export class SheetLinkedDropdownCell {
  public readonly cell = input.required<SheetCell>();
  /** What the cell's column offers right now. */
  public readonly choices = input.required<LinkedChoices>();
  /** The cell's caption, shown as a hint while nothing is chosen. */
  public readonly label = input('');

  /** The user moved into the cell to edit it. */
  public readonly started = output<void>();
  public readonly changed = output<CellValueRequest>();

  public readonly value = linkedSignal<string | null>(() => this.cell().textValue);

  /** The column's values, with the cell's own value kept in the list if the column no longer has it. */
  public readonly options = computed(() => [
    ...linkedOptions(this.choices(), this.cell().textValue),
  ]);

  public readonly isReady = computed(() => this.choices().status === 'ready');

  public readonly hasFilter = computed(() => this.options().length > FILTER_FROM);

  public readonly placeholder = computed(() => linkedHint(this.choices()) ?? this.label());

  public choose(value: string | null): void {
    this.value.set(value);
    if (value !== this.cell().textValue) {
      this.changed.emit(valueRequest(this.cell(), 'LinkedDropdown', value));
    }
  }
}
