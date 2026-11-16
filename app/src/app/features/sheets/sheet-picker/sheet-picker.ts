import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SelectModule } from 'primeng/select';
import { SheetSummary } from '../../../core/models/sheet-summary.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { SheetIdentityUtil } from '../../../shared/sheet-identity.util';
import { SheetPickerOption } from '../models/sheet-picker-option.model';

/**
 * Which of the phase's sheets of a type is open, named by its identifier values, for a sheet type that
 * can have many. Choosing another opens it.
 */
@Component({
  selector: 'app-sheet-picker',
  imports: [FormsModule, SelectModule],
  templateUrl: './sheet-picker.html',
  styleUrl: './sheet-picker.scss',
})

export class SheetPicker {
  public readonly sheetType = input.required<SheetType>();
  /** The phase's sheets, of every type. */
  public readonly sheets = input.required<SheetSummary[]>();
  public readonly currentId = input<number | null>(null);

  public readonly options = computed<SheetPickerOption[]>(() => {
    const sheetType = this.sheetType();
    return this.sheets()
      .filter((sheet) => sheet.sheetTypeId === sheetType.id)
      .map((sheet) => ({ id: sheet.id, label: SheetIdentityUtil.label(sheet, sheetType), link: SheetIdentityUtil.link(sheet) }));
  });

  public readonly ariaLabel = computed(() => `${this.sheetType().name} sheet`);

  private readonly router = inject(Router);

  public open(sheetId: number | null): void {
    const option = this.options().find((candidate) => candidate.id === sheetId);
    if (option && option.id !== this.currentId()) {
      void this.router.navigate(option.link);
    }
  }
}
