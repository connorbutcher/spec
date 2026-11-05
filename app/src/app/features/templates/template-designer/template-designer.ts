import { afterNextRender, Component, inject, viewChild } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { CellTypesStore } from '../cell-types.store';
import { DesignerToolbar } from '../designer-toolbar/designer-toolbar';
import { TableTemplate } from '../models/table-template.model';
import { TemplateLayout } from '../models/template-layout.model';
import { PanelCrumbsUtil } from '../panel-crumbs.util';
import { PanelNavigator } from '../panel-navigator';
import { SelectionBar } from '../selection-bar/selection-bar';
import { StructureMenu } from '../structure-menu';
import { StructureOutline } from '../structure-outline/structure-outline';
import { TemplateCanvas } from '../template-canvas/template-canvas';
import { TemplatesStore } from '../templates.store';

/**
 * The middle column (`/templates/:templateId`): the toolbar, the table's whole structure as a tree on
 * the left, and beside it the selection bar over a clean preview of the table. Selecting anything in
 * the tree or the preview opens its settings on the right; Esc selects its parent.
 */
@Component({
  selector: 'app-template-designer',
  imports: [
    ButtonModule,
    ContextMenuModule,
    DesignerToolbar,
    EmptyState,
    SelectionBar,
    StructureOutline,
    TemplateCanvas,
  ],
  providers: [StructureMenu],
  templateUrl: './template-designer.html',
  styleUrl: './template-designer.scss',
  host: {
    '(document:keydown.escape)': 'selectParent($event)',
  },
})
export class TemplateDesigner {
  public readonly menu = inject(StructureMenu);

  private readonly store = inject(TemplatesStore);
  private readonly cellTypes = inject(CellTypesStore);
  private readonly navigator = inject(PanelNavigator);
  private readonly contextMenu = viewChild.required(ContextMenu);

  constructor() {
    afterNextRender(() => this.menu.register(this.contextMenu()));
  }

  public template(): TableTemplate | null {
    return this.store.template();
  }

  public layout(): TemplateLayout | null {
    return this.store.layout();
  }

  public isLoading(): boolean {
    return this.store.templateIsLoading() && this.store.template() === null;
  }

  public hasError(): boolean {
    return this.store.templateHasError();
  }

  public retry(): void {
    this.store.reloadTemplate();
  }

  /** Esc moves the selection up a level, unless it's closing something or editing a field. */
  public selectParent(event: Event): void {
    const target = event.target as HTMLElement | null;
    if (
      event.defaultPrevented ||
      target?.closest('input, textarea, select, [role="dialog"], .p-overlay')
    ) {
      return;
    }
    const crumbs = PanelCrumbsUtil.panelCrumbs(
      this.navigator.current(),
      this.store.template(),
      this.store.index(),
      (id) => this.cellTypes.cellType(id),
    );
    const parent = crumbs.at(-2);
    if (parent) {
      this.navigator.open(parent.ref);
    }
  }
}
