import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { TooltipModule } from 'primeng/tooltip';
import { CellTypesStore } from '../cell-types.store';
import { PanelNavigator } from '../panel-navigator';
import { TemplateListPanel } from '../template-list-panel/template-list-panel';
import { TemplatesStore } from '../templates.store';

/**
 * While a table is open, the table list folds into this narrow rail so the designer has the room. Its
 * "Tables" button slides the full list out as a PrimeNG drawer (closing again once a table is
 * picked), and "Cell types" opens them in the settings panel.
 */
@Component({
  selector: 'app-templates-rail',
  imports: [ButtonModule, DrawerModule, TemplateListPanel, TooltipModule],
  templateUrl: './templates-rail.html',
  styleUrl: './templates-rail.scss',
})
export class TemplatesRail {
  public readonly isOpen = signal(false);

  public readonly cellTypesLabel = computed(
    () => `Cell types (${this.cellTypes.cellTypes().length})`,
  );

  private readonly store = inject(TemplatesStore);
  private readonly cellTypes = inject(CellTypesStore);
  private readonly navigator = inject(PanelNavigator);

  constructor() {
    effect(() => {
      this.store.selectedTemplateId();
      untracked(() => this.isOpen.set(false));
    });
  }

  public isShowingCellTypes(): boolean {
    const current = this.navigator.current();
    return current.kind === 'cellTypes' || current.kind === 'cellType';
  }

  public openCellTypes(): void {
    this.navigator.open({ kind: 'cellTypes' });
  }
}
