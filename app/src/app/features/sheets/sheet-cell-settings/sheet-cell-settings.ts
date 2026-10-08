import { Component, computed, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { TooltipModule } from 'primeng/tooltip';
import { CellType } from '../../templates/models/cell-type.model';
import { CellInstanceSettings } from '../models/cell-instance-settings';
import { CellSettingsRequest } from '../models/cell-settings-request.model';
import { SheetCell } from '../models/sheet-cell.model';
import { SheetLinkedSourceForm } from '../sheet-linked-source-form/sheet-linked-source-form';

/** What the button says for each kind of cell that has settings on the sheet. */
const TITLES: Readonly<Record<string, string>> = {
  LinkedDropdown: 'Choose where the choices come from',
};

/**
 * The button beside the editor of a cell whose kind has settings chosen on the sheet, and the popover it
 * opens. It is the one place a kind is mapped to the form for its settings: to give another kind
 * settings, add its form component and a `@case` here. What a form applies is sent on as a request for
 * this cell; saving it is the grid cell's job, as it is for values.
 */
@Component({
  selector: 'app-sheet-cell-settings',
  imports: [ButtonModule, PopoverModule, SheetLinkedSourceForm, TooltipModule],
  templateUrl: './sheet-cell-settings.html',
  styleUrl: './sheet-cell-settings.scss',
})
export class SheetCellSettings {
  public readonly cell = input.required<SheetCell>();
  public readonly cellType = input.required<CellType>();

  public readonly changed = output<CellSettingsRequest>();

  public readonly kind = computed(() => this.cellType().kind);

  public readonly title = computed(() => TITLES[this.kind()] ?? 'Cell settings');

  /** The cell's settings when they are a linked dropdown's, for that form. */
  public readonly linkedDropdown = computed(() => {
    const settings = this.cell().settings;
    return settings?.kind === 'LinkedDropdown' ? settings : null;
  });

  public save(settings: CellInstanceSettings | null, popover: Popover): void {
    popover.hide();
    this.changed.emit({ sheetCellId: this.cell().id, settings });
  }
}
