import { Component, computed, input } from '@angular/core';
import { SheetType } from '../../../core/models/sheet-type.model';
import { sheetTypeIcon } from '../sheet-type-icon';

/** One available sheet type. Will open the phase's sheet of this type once sheets exist. */
@Component({
  selector: 'app-sheet-type-card',
  template: `
    <div class="sheet-type-card">
      <span class="icon" aria-hidden="true">
        <i class="pi" [class]="icon()"></i>
      </span>
      <span class="name">{{ sheetType().name }}</span>
    </div>
  `,
  styleUrl: './sheet-type-card.component.scss',
})
export class SheetTypeCardComponent {
  public readonly sheetType = input.required<SheetType>();

  public readonly icon = computed(() => sheetTypeIcon(this.sheetType().name));
}
