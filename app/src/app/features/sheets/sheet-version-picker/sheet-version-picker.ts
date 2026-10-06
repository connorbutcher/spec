import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { SheetVersionOption } from '../models/sheet-version-option.model';
import { momentLabel, versionLabel } from '../sheet-labels.util';
import { SheetStore } from '../sheet.store';

/** Marks the "custom date" entry, which only appears while a date is being viewed. */
const DATE_OPTION = -1;

/**
 * Chooses which moment of the sheet to look at: the live sheet, the state when a version was published,
 * or the state at a date and time, which works whatever has been published since. Past views are read-only.
 */
@Component({
  selector: 'app-sheet-version-picker',
  imports: [ButtonModule, DatePickerModule, FormsModule, SelectModule],
  templateUrl: './sheet-version-picker.html',
  styleUrl: './sheet-version-picker.scss',
})
export class SheetVersionPicker {
  public readonly options = computed<SheetVersionOption[]>(() => {
    const options: SheetVersionOption[] = [{ label: 'Latest (live)', value: null }];
    if (this.store.view().asOf) {
      options.push({ label: 'Date and time', value: DATE_OPTION });
    }
    for (const version of this.store.versions()) {
      const published = momentLabel(version.publishedAtUtc);
      options.push({
        label: `${versionLabel(version.versionNumber)} · ${published} · ${version.publishedByName}`,
        value: version.versionNumber,
      });
    }
    return options;
  });

  public readonly selected = computed<number | null>(() => {
    const view = this.store.view();
    return view.asOf ? DATE_OPTION : (view.version ?? null);
  });

  public readonly asOf = computed<Date | null>(() => {
    const asOf = this.store.view().asOf;
    return asOf ? new Date(asOf) : null;
  });

  public readonly isLive = computed(() => this.store.sheet()?.isLive ?? true);

  public readonly hasVersions = computed(() => this.store.versions().length > 0);

  /** A date can't be picked from the future. */
  public readonly today = new Date();

  private readonly store = inject(SheetStore);

  public chooseVersion(value: number | null): void {
    if (value === DATE_OPTION) {
      return;
    }
    this.store.setView(value === null ? {} : { version: value });
  }

  public chooseDate(date: Date | null): void {
    this.store.setView(date === null ? {} : { asOf: date.toISOString() });
  }

  public backToLive(): void {
    this.store.setView({});
  }
}
