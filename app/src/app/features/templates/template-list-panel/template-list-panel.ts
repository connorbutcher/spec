import { Component, inject } from '@angular/core';
import { SheetType } from '../../../core/models/sheet-type.model';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { PanelNavigator } from '../panel-navigator';
import { TemplateListGroup } from '../template-list-group/template-list-group';
import { TemplatesStore } from '../templates.store';

/** The left-hand card: each sheet type with its tables, and a way into the cell types. */
@Component({
  selector: 'app-template-list-panel',
  imports: [EmptyState, TemplateListGroup],
  templateUrl: './template-list-panel.html',
  styleUrl: './template-list-panel.scss',
})
export class TemplateListPanel {
  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public sheetTypes(): SheetType[] {
    return this.store.sheetTypes();
  }

  public isLoading(): boolean {
    return this.store.listIsLoading() && this.store.sheetTypes().length === 0;
  }

  public hasError(): boolean {
    return this.store.listHasError();
  }

  public cellTypeCount(): number {
    return this.store.cellTypes().length;
  }

  public isShowingCellTypes(): boolean {
    const current = this.navigator.current();
    return current.kind === 'cellTypes' || current.kind === 'cellType';
  }

  public openCellTypes(): void {
    this.navigator.open({ kind: 'cellTypes' });
  }

  public retry(): void {
    this.store.reloadList();
  }
}
