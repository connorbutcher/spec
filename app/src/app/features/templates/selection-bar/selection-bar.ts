import { Component, computed, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { CellTypesStore } from '../cell-types.store';
import { PanelCrumb } from '../models/panel-crumb.model';
import { PanelRef } from '../models/panel-ref';
import { StructureAction } from '../models/structure-action.model';
import { PanelBreadcrumb } from '../panel-breadcrumb/panel-breadcrumb';
import { PanelCrumbsUtil } from '../panel-crumbs.util';
import { PanelNavigator } from '../panel-navigator';
import { StructureActions } from '../structure-actions';
import { TemplatesStore } from '../templates.store';

/**
 * The line above the preview: where the selection sits in the table (each step selects its level),
 * and what can be done to it: adds as labelled buttons, then moves and inserts as icons, then delete.
 * With the cell types open it shows the table and its actions.
 */
@Component({
  selector: 'app-selection-bar',
  imports: [ButtonModule, PanelBreadcrumb, TooltipModule],
  templateUrl: './selection-bar.html',
  styleUrl: './selection-bar.scss',
})
export class SelectionBar {
  /** What's selected in the table; the cell types aren't part of it, so they count as the table. */
  public readonly selection = computed<PanelRef>(() => {
    const current = this.navigator.current();
    return current.kind === 'cellTypes' || current.kind === 'cellType'
      ? { kind: 'template' }
      : current;
  });

  public readonly crumbs = computed<PanelCrumb[]>(() =>
    PanelCrumbsUtil.panelCrumbs(this.selection(), this.store.template(), this.store.index(), (id) =>
      this.cellTypes.cellType(id),
    ),
  );

  public readonly actions = computed<StructureAction[]>(() =>
    this.structureActions.actionsFor(this.selection()),
  );

  public readonly labelled = computed(() =>
    this.actions().filter((action) => !action.iconOnly && !action.danger),
  );

  public readonly icons = computed(() =>
    this.actions().filter((action) => action.iconOnly && !action.danger),
  );

  public readonly danger = computed(() => this.actions().filter((action) => action.danger));

  private readonly store = inject(TemplatesStore);
  private readonly cellTypes = inject(CellTypesStore);
  private readonly navigator = inject(PanelNavigator);
  private readonly structureActions = inject(StructureActions);

  public run(action: StructureAction, event: Event): void {
    action.run(event.currentTarget);
  }
}
