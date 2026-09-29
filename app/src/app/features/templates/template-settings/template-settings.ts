import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectButtonChangeEvent, SelectButtonModule } from 'primeng/selectbutton';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { TableTemplate } from '../models/table-template.model';
import { TemplateOrientation } from '../models/template-orientation';
import { PanelLinkList } from '../panel-link-list/panel-link-list';
import { PanelNavigator } from '../panel-navigator';
import { sectionLinks } from '../section-links.util';
import { topLevelOrder } from '../template-layout.util';
import { TemplatesStore } from '../templates.store';

/** Panel page for the open table: its name, orientation, version and top-level sections. */
@Component({
  selector: 'app-template-settings',
  imports: [
    ButtonModule,
    ConfirmDeleteButton,
    FormsModule,
    InputTextModule,
    PanelLinkList,
    SelectButtonModule,
  ],
  templateUrl: './template-settings.html',
  styleUrl: './template-settings.scss',
})
export class TemplateSettings {
  public readonly template = computed<TableTemplate | null>(() => this.store.template());

  public readonly sections = computed<PanelLinkItem[]>(() =>
    sectionLinks(topLevelOrder(this.template()?.sections ?? [])),
  );

  public readonly orientations = [
    { label: 'Horizontal', value: 'Horizontal' },
    { label: 'Vertical', value: 'Vertical' },
  ];

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  public async rename(input: HTMLInputElement): Promise<void> {
    const template = this.template();
    const name = input.value.trim();
    if (template && name && name !== template.name) {
      await this.store.updateTemplate(name, template.orientation);
    }
    input.value = this.template()?.name ?? '';
  }

  public setOrientation(event: SelectButtonChangeEvent): void {
    const template = this.template();
    const orientation = event.value as TemplateOrientation | null;
    if (template && orientation && orientation !== template.orientation) {
      void this.store.updateTemplate(template.name, orientation);
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
