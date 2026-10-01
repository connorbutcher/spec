import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { DesignerToolbar } from '../designer-toolbar/designer-toolbar';
import { TableTemplate } from '../models/table-template.model';
import { TemplateLayout } from '../models/template-layout.model';
import { StructureOutline } from '../structure-outline/structure-outline';
import { TemplateCanvas } from '../template-canvas/template-canvas';
import { TemplatesStore } from '../templates.store';

/**
 * The middle column (`/templates/:templateId`): the toolbar, an outline of the table's structure where
 * sections and sub-sections are added, and a clean preview of the whole table. Selecting anything opens
 * its settings on the right.
 */
@Component({
  selector: 'app-template-designer',
  imports: [ButtonModule, DesignerToolbar, EmptyState, StructureOutline, TemplateCanvas],
  templateUrl: './template-designer.html',
  styleUrl: './template-designer.scss',
})
export class TemplateDesigner {
  private readonly store = inject(TemplatesStore);

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
}
