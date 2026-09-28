import { Component, computed, input } from '@angular/core';
import { SheetType } from '../../../core/models/sheet-type.model';
import { sheetTypeAppearance } from '../sheet-type-appearance';

/**
 * One PU Spec Sheet for the phase, shown as a full-width section with its position, icon and accent
 * colour. The body is where the sheet's status and summary will go once sheets exist.
 */
@Component({
  selector: 'app-sheet-type-section',
  templateUrl: './sheet-type-section.html',
  styleUrl: './sheet-type-section.scss',
  host: {
    '[style.--sheet-accent]': 'appearance().accent',
  },
})
export class SheetTypeSection {
  public readonly sheetType = input.required<SheetType>();
  public readonly position = input.required<number>();
  public readonly phaseCode = input.required<string>();

  public readonly appearance = computed(() => sheetTypeAppearance(this.sheetType().name));
  public readonly positionLabel = computed(() => String(this.position()).padStart(2, '0'));
}
