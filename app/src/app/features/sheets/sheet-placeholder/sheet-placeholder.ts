import { Component, input } from '@angular/core';
import { SheetType } from '../../../core/models/sheet-type.model';

/** Where a phase's sheet content will render once table templates exist. */
@Component({
  selector: 'app-sheet-placeholder',
  templateUrl: './sheet-placeholder.html',
  styleUrl: './sheet-placeholder.scss',
})
export class SheetPlaceholder {
  public readonly sheetType = input.required<SheetType>();
  public readonly phaseCode = input.required<string>();
}
