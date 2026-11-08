import {
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  input,
  linkedSignal,
  output,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { CELL_INPUT_STYLE } from '../layout/cell-input-style';
import { CellValueUtil } from '../values/cell-value.util';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';

/**
 * The editor for a number cell: a PrimeNG number box that fills the whole cell, set up with the cell's
 * decimal places, limits and unit. The value is saved when focus leaves it or on Enter; Escape puts
 * back the saved value. Nothing is emitted if the number hasn't changed. The arrow keys don't step the
 * number up or down: on a sheet of limits that turned a stray key press into a changed value.
 */
@Component({
  selector: 'app-sheet-number-cell',
  imports: [FormsModule, InputNumberModule],
  templateUrl: './sheet-number-cell.html',
  styleUrl: './sheet-number-cell.scss',
  host: {
    '(keydown.escape)': 'revert()',
  },
})

export class SheetNumberCell {
  public readonly cell = input.required<SheetCell>();
  public readonly configuration = input.required<CellConfiguration>();
  /** The cell's caption, shown as a hint while the cell is empty. */
  public readonly label = input('');

  /** The user moved into the cell to edit it. */
  public readonly started = output<void>();
  public readonly changed = output<CellValueRequest>();

  public readonly number = linkedSignal<number | null>(() => this.cell().numberValue);

  public readonly inputStyle = CELL_INPUT_STYLE;

  public readonly settings = computed(() => {
    const configuration = this.configuration();
    return configuration.kind === 'Number' ? configuration : null;
  });

  /** The unit shown after the number, e.g. " Nm". */
  public readonly suffix = computed(() => {
    const unit = this.settings()?.unit;
    return unit ? ` ${unit}` : '';
  });

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Listens in the capture phase, so it runs before PrimeNG's own key handler on the input. */
  constructor() {
    const element = this.host.nativeElement;
    element.addEventListener('keydown', SheetNumberCell.stopArrowStep, { capture: true });
    inject(DestroyRef).onDestroy(() =>
      element.removeEventListener('keydown', SheetNumberCell.stopArrowStep, { capture: true }),
    );
  }

  private static stopArrowStep(event: KeyboardEvent): void {
    if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
      event.stopPropagation();
    }
  }

  public commit(): void {
    const value = this.number();
    if (value !== this.cell().numberValue) {
      this.changed.emit(CellValueUtil.valueRequest(this.cell(), 'Number', value));
    }
  }

  public revert(): void {
    this.number.set(this.cell().numberValue);
  }
}
