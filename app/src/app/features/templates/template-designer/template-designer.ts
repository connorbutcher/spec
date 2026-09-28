import { Component, inject } from '@angular/core';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { DesignerToolbar } from '../designer-toolbar/designer-toolbar';
import { TableTemplate } from '../models/table-template.model';
import { TemplateLayout } from '../models/template-layout.model';
import { PanelNavigator } from '../panel-navigator';
import { TemplateCanvas } from '../template-canvas/template-canvas';
import { TemplatesStore } from '../templates.store';

/** The middle column (`/templates/:templateId`): the open table's toolbar and a live preview of its grid. */
@Component({
  selector: 'app-template-designer',
  imports: [DesignerToolbar, EmptyState, TemplateCanvas],
  templateUrl: './template-designer.html',
  styleUrl: './template-designer.scss',
})
export class TemplateDesigner {
  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

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

  public async addFirstSection(): Promise<void> {
    const id = await this.store.addSection(null);
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }
}
