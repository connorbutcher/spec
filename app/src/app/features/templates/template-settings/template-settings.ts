import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectButtonChangeEvent, SelectButtonModule } from 'primeng/selectbutton';
import { columnBlockLinks } from '../column-block-links.util';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { TableTemplate } from '../models/table-template.model';
import { TemplateOrientation } from '../models/template-orientation';
import { PanelLinkList } from '../panel-link-list/panel-link-list';
import { PanelNavigator } from '../panel-navigator';
import { StickyColumnsField } from '../sticky-columns-field/sticky-columns-field';
import { sectionLinks } from '../section-links.util';
import { planColumns, topLevelOrder } from '../template-layout.util';
import { TemplatesStore } from '../templates.store';

/**
 * Panel page for the open table: its name, orientation, version, top-level sections and, for a
 * horizontal table, its column blocks.
 */
@Component({
  selector: 'app-template-settings',
  imports: [
    ButtonModule,
    ConfirmDeleteButton,
    FormsModule,
    InputTextModule,
    PanelLinkList,
    SelectButtonModule,
    StickyColumnsField,
  ],
  templateUrl: './template-settings.html',
  styleUrl: './template-settings.scss',
})
export class TemplateSettings {
  public readonly template = computed<TableTemplate | null>(() => this.store.template());

  public readonly sections = computed<PanelLinkItem[]>(() =>
    sectionLinks(topLevelOrder(this.template()?.sections ?? [])),
  );

  public readonly columnBlocks = computed<PanelLinkItem[]>(() => columnBlockLinks(this.template()));

  /** How many columns the table's own cells take, before the column blocks: the most that can be pinned. */
  public readonly ownColumns = computed(() => {
    const template = this.template();
    if (!template) {
      return 1;
    }
    const plan = planColumns(template);
    const firstBlock = Math.min(...plan.blockStarts.values(), plan.total + 1);
    return Math.max(1, firstBlock - 1);
  });

  public readonly orientations = [
    { label: 'Horizontal', value: 'Horizontal' },
    { label: 'Vertical', value: 'Vertical' },
  ];

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  /** Only horizontal tables have column blocks. */
  public isHorizontal(): boolean {
    return this.template()?.orientation === 'Horizontal';
  }

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

  public setSticky(count: number): void {
    const template = this.template();
    if (template) {
      void this.store.updateTemplate(template.name, template.orientation, count);
    }
  }

  public async addSection(): Promise<void> {
    const id = await this.store.addSection(null);
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }

  public async addColumnBlock(): Promise<void> {
    const id = await this.store.addColumnBlock();
    if (id !== null) {
      this.navigator.open({ kind: 'columnBlock', id });
    }
  }

  public delete(): void {
    void this.store.deleteTemplate();
  }
}
