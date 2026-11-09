import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { SheetComparePicker } from '../sheet-compare-picker/sheet-compare-picker';
import { SheetLabelsUtil } from '../status/sheet-labels.util';
import { SheetVersionPicker } from '../sheet-version-picker/sheet-version-picker';
import { SheetStore } from '../sheet.store';

/**
 * What is being looked at, as one button: "Live · v9", or "v6 · read-only" for a past view, plus the
 * version changes are marked since. It opens the controls that choose those (version, date and time,
 * compare) and reloading the sheet. Away from live, a second button goes straight back.
 */
@Component({
  selector: 'app-sheet-view-menu',
  imports: [ButtonModule, PopoverModule, SheetComparePicker, SheetVersionPicker],
  templateUrl: './sheet-view-menu.html',
  styleUrl: './sheet-view-menu.scss',
})

export class SheetViewMenu {
  public readonly label = computed(() => {
    const sheet = this.store.sheet();
    return sheet === null ? '' : SheetLabelsUtil.viewLabel(sheet, this.store.changesSince());
  });

  public readonly isLive = computed(() => this.store.sheet()?.isLive ?? true);

  public readonly hasVersions = computed(() => this.store.versions().length > 0);

  private readonly store = inject(SheetStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  /** Shows the live sheet, dropping any submission the address asked to review so a reload stays live. */
  public backToLive(): void {
    this.store.setView({});
    void this.router.navigate([], { relativeTo: this.route, queryParams: { review: null }, queryParamsHandling: 'merge' });
  }

  public reload(popover: Popover): void {
    popover.hide();
    this.store.reload();
  }
}
