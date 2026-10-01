import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { SheetVersionOption } from '../models/sheet-version-option.model';
import { SheetStore } from '../sheet.store';

/**
 * Turns on the "changed in" markers: pick a version and everything that changed after it is marked on
 * the tables. Off leaves the tables completely clean.
 */
@Component({
  selector: 'app-sheet-compare-picker',
  imports: [FormsModule, SelectModule],
  templateUrl: './sheet-compare-picker.html',
  styleUrl: './sheet-compare-picker.scss',
})
export class SheetComparePicker {
  public readonly options = computed<SheetVersionOption[]>(() => {
    const versions = [...(this.store.sheet()?.versions ?? [])].sort(
      (a, b) => b.versionNumber - a.versionNumber,
    );
    return [
      { label: 'Changes: off', value: null },
      ...versions.map((version) => ({
        label: `Changes since v${version.versionNumber}`,
        value: version.versionNumber,
      })),
    ];
  });

  public readonly selected = computed(() => this.store.changesSince());

  public readonly hasVersions = computed(() => (this.store.sheet()?.versions.length ?? 0) > 0);

  private readonly store = inject(SheetStore);

  public choose(value: number | null): void {
    this.store.setChangesSince(value);
  }
}
