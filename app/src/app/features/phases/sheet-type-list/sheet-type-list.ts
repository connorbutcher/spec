import { Component, input } from '@angular/core';
import { SheetType } from '../../../core/models/sheet-type.model';
import { SheetTypeSection } from '../sheet-type-section/sheet-type-section';

/** The phase's PU Spec Sheets, one full-width section per available sheet type, in order. */
@Component({
  selector: 'app-sheet-type-list',
  imports: [SheetTypeSection],
  templateUrl: './sheet-type-list.html',
  styleUrl: './sheet-type-list.scss',
})
export class SheetTypeList {
  public readonly sheetTypes = input.required<SheetType[]>();
  public readonly phaseCode = input.required<string>();
}
