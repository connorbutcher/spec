import { Component, computed, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { CellSettings } from '../cell-settings/cell-settings';
import { CellTypeList } from '../cell-type-list/cell-type-list';
import { CellTypeSettings } from '../cell-type-settings/cell-type-settings';
import { PanelCrumb } from '../models/panel-crumb.model';
import { PanelRef } from '../models/panel-ref';
import { panelCrumbs } from '../panel-crumbs.util';
import { PanelBreadcrumb } from '../panel-breadcrumb/panel-breadcrumb';
import { PanelNavigator } from '../panel-navigator';
import { RowSettings } from '../row-settings/row-settings';
import { SectionSettings } from '../section-settings/section-settings';
import { TemplateSettings } from '../template-settings/template-settings';
import { TemplatesStore } from '../templates.store';

const TITLES: Readonly<Record<PanelRef['kind'], string>> = {
  template: 'Table',
  section: 'Section',
  row: 'Row',
  cell: 'Cell',
  cellTypes: 'Cell types',
  cellType: 'Cell type',
};

/**
 * The right-hand configuration panel. Shows the settings for whatever was last opened, with back and
 * forward through what's been opened and a breadcrumb up through the table.
 */
@Component({
  selector: 'app-config-panel',
  imports: [
    ButtonModule,
    MessageModule,
    TooltipModule,
    CellSettings,
    CellTypeList,
    CellTypeSettings,
    PanelBreadcrumb,
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
    panelCrumbs(this.current(), this.store.template(), this.store.index(), (id) =>
      this.store.cellType(id),
    ),
  );

  private readonly navigator = inject(PanelNavigator);
  private readonly store = inject(TemplatesStore);

  public title(): string {
    return TITLES[this.current().kind];
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

  public canGoBack(): boolean {
    return this.navigator.canGoBack();
  }

  public canGoForward(): boolean {
    return this.navigator.canGoForward();
  }

  public back(): void {
    this.navigator.back();
  }

  public forward(): void {
    this.navigator.forward();
  }

  /** The id of the current item, for panels that show one. */
  public id(): number {
    const current = this.current();
    return 'id' in current ? current.id : 0;
  }
}
