import { Component, inject, input } from '@angular/core';
import { TableTemplate } from '../models/table-template.model';
import { TemplateOrientation } from '../models/template-orientation';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

/** The bar above the canvas: the table's name, its orientation and adding a top-level section. */
@Component({
  selector: 'app-designer-toolbar',
  templateUrl: './designer-toolbar.html',
  styleUrl: './designer-toolbar.scss',
})
export class DesignerToolbar {
  public readonly template = input.required<TableTemplate>();

  public readonly orientations: readonly TemplateOrientation[] = ['Horizontal', 'Vertical'];

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public isTemplateOpen(): boolean {
    return this.navigator.current().kind === 'template';
  }

  public openSettings(): void {
    this.navigator.open({ kind: 'template' });
  }

  public setOrientation(orientation: TemplateOrientation): void {
    if (orientation !== this.template().orientation) {
      void this.store.updateTemplate(this.template().name, orientation);
    }
  }

  public async addSection(): Promise<void> {
    const id = await this.store.addSection(null);
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }
}
