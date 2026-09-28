import { Component, computed, input } from '@angular/core';
import { SheetType } from '../../../core/models/sheet-type.model';
import { sheetTypeIcon } from '../sheet-type-icon';

/** One PU Spec Sheet row for the phase: its position, icon and name. */
@Component({
  selector: 'app-sheet-type-section',
  templateUrl: './sheet-type-section.html',
  styleUrl: './sheet-type-section.scss',
})
export class SheetTypeSection {
  public readonly sheetType = input.required<SheetType>();
  public readonly position = input.required<number>();
  public readonly phaseCode = input.required<string>();

  public readonly icon = computed(() => sheetTypeIcon(this.sheetType().name));
}
