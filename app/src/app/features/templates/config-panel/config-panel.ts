import { Component, computed, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ErrorMessage } from '../../../shared/components/error-message/error-message';
import { CellSettings } from '../cell-settings/cell-settings';
import { CellTypeList } from '../cell-type-list/cell-type-list';
import { CellTypeSettings } from '../cell-type-settings/cell-type-settings';
import { ColumnBlockSettings } from '../column-block-settings/column-block-settings';
import { PanelCrumb } from '../models/panel-crumb.model';
import { PanelRef } from '../models/panel-ref';
import { StructureLevel } from '../models/structure-level';
import { PanelCrumbsUtil } from '../panel-crumbs.util';
import { PanelNavigator } from '../panel-navigator';
import { RowSettings } from '../row-settings/row-settings';
import { SectionSettings } from '../section-settings/section-settings';
import { StructureIcons } from '../structure-icons';
import { TemplateSettings } from '../template-settings/template-settings';
import { CellTypesStore } from '../cell-types.store';
import { TemplatesStore } from '../templates.store';

/**
 * The right-hand configuration panel: the settings for whatever is selected, under a one-line title
 * with its level's icon and name. Where it sits in the table is in the selection bar.
 */
@Component({
  selector: 'app-config-panel',
  imports: [
    ButtonModule,
    CellSettings,
    CellTypeList,
    CellTypeSettings,
    ColumnBlockSettings,
    ErrorMessage,
    RowSettings,
    SectionSettings,
    TemplateSettings,
  ],
  templateUrl: './config-panel.html',
  styleUrl: './config-panel.scss',
})
export class ConfigPanel {
  public readonly current = computed<PanelRef>(() => this.navigator.current());

  public readonly crumbs = computed<PanelCrumb[]>(() =>
    PanelCrumbsUtil.panelCrumbs(this.current(), this.store.template(), this.store.index(), (id) =>
      this.cellTypeStore.cellType(id),
    ),
  );

  public readonly level = computed<StructureLevel>(() =>
    StructureIcons.levelOf(this.current(), this.store.index()),
  );

  /** The selected item's own name, after its noun; none for the cell type list. */
  public readonly name = computed<string | null>(() => {
    const kind = this.current().kind;
    return kind === 'cellTypes' ? null : (this.crumbs().at(-1)?.label ?? null);
  });

  private readonly navigator = inject(PanelNavigator);
  private readonly store = inject(TemplatesStore);
  private readonly cellTypeStore = inject(CellTypesStore);

  public noun(): string {
    return StructureIcons.noun(this.level());
  }

  public icon(): string {
    const level = this.level();
    return `pi ${StructureIcons.icon(level)} ${StructureIcons.levelClass(level)}`;
  }

  /** False when the item was deleted or its table isn't loaded yet. */
  public exists(): boolean {
    return this.crumbs().length > 0;
  }

  public error(): string | null {
    return this.store.error();
  }

  public dismissError(): void {
    this.store.dismissError();
  }

  public openCellTypes(): void {
    this.navigator.open({ kind: 'cellTypes' });
  }

  /** The id of the current item, for panels that show one. */
  public id(): number {
    const current = this.current();
    return 'id' in current ? current.id : 0;
  }
}
