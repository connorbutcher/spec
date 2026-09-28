import { Component, computed, inject } from '@angular/core';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { TableTemplate } from '../models/table-template.model';
import { TemplateOrientation } from '../models/template-orientation';
import { PanelLinkList } from '../panel-link-list/panel-link-list';
import { PanelNavigator } from '../panel-navigator';
import { sectionLinks } from '../section-links.util';
import { TemplatesStore } from '../templates.store';

/** Panel page for the open table: its name, orientation and top-level sections. */
@Component({
  selector: 'app-template-settings',
  imports: [ConfirmDeleteButton, PanelLinkList],
  templateUrl: './template-settings.html',
  styleUrl: './template-settings.scss',
})
export class TemplateSettings {
  public readonly template = computed<TableTemplate | null>(() => this.store.template());

  public readonly sections = computed<PanelLinkItem[]>(() =>
    sectionLinks(this.template()?.sections ?? []),
  );

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public async rename(input: HTMLInputElement): Promise<void> {
    const template = this.template();
    const name = input.value.trim();
    if (template && name && name !== template.name) {
      await this.store.updateTemplate(name, template.orientation);
    }
    input.value = this.template()?.name ?? '';
  }

  public setOrientation(value: string): void {
    const template = this.template();
    if (template && value !== template.orientation) {
      void this.store.updateTemplate(template.name, value as TemplateOrientation);
    }
  }

  public async addSection(): Promise<void> {
    const id = await this.store.addSection(null);
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }

  public delete(): void {
    void this.store.deleteTemplate();
  }
}
