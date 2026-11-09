import { Component, computed, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { TooltipModule } from 'primeng/tooltip';
import { CellConfiguration } from '../../templates/models/cell-configuration';
import { CellType } from '../../templates/models/cell-type.model';
import { LabelledTextSlot } from '../../templates/models/labelled-text-slot.model';
import { LabelledTextSlotsUtil } from '../../templates/labelled-text-slots.util';
import { CellInstanceSettings } from '../models/cell-instance-settings';
import { CellSettingsRequest } from '../models/cell-settings-request.model';
import { SheetCell } from '../models/sheet-cell.model';
import { SheetLabelledTextSlotsForm } from '../sheet-labelled-text-slots-form/sheet-labelled-text-slots-form';
import { SheetLinkedSourceForm } from '../sheet-linked-source-form/sheet-linked-source-form';

/** What the button says for each kind of cell that has settings on the sheet. */
const TITLES: Readonly<Record<string, string>> = {
  LinkedDropdown: 'Choose where the choices come from',
  LabelledText: 'Change the values this cell holds',
};

/** The button's icon for each kind of cell that has settings on the sheet. */
const ICONS: Readonly<Record<string, string>> = {
  LinkedDropdown: 'pi pi-link',
  LabelledText: 'pi pi-tags',
};

/**
 * The button beside the editor of a cell whose kind has settings chosen on the sheet, and the popover it
 * opens. It is the one place a kind is mapped to the form for its settings: to give another kind
 * settings, add its form component and a `@case` here. What a form applies is sent on as a request for
 * this cell; saving it is the grid cell's job, as it is for values.
 */
@Component({
  selector: 'app-sheet-cell-settings',
  imports: [ButtonModule, PopoverModule, SheetLabelledTextSlotsForm, SheetLinkedSourceForm, TooltipModule],
  templateUrl: './sheet-cell-settings.html',
  styleUrl: './sheet-cell-settings.scss',
})
export class SheetCellSettings {
  public readonly cell = input.required<SheetCell>();
  public readonly cellType = input.required<CellType>();
  /** The cell type's settings with the cell's own overrides applied. */
  public readonly configuration = input.required<CellConfiguration>();

  public readonly changed = output<CellSettingsRequest>();

  public readonly kind = computed(() => this.cellType().kind);

  public readonly title = computed(() => TITLES[this.kind()] ?? 'Cell settings');

  public readonly icon = computed(() => ICONS[this.kind()] ?? 'pi pi-cog');

  /** For a labelled text cell, the values its template gives it. */
  public readonly templateSlots = computed<readonly LabelledTextSlot[]>(() =>
    LabelledTextSlotsUtil.templateSlots(this.configuration()),
  );

  /** For a labelled text cell, the values chosen for it on the sheet, or null when it uses the template's. */
  public readonly chosenSlots = computed<readonly LabelledTextSlot[] | null>(() => {
    const settings = this.cell().settings;
    return settings?.kind === 'LabelledText' ? settings.slots : null;
  });

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
