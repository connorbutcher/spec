import { Component, computed, inject } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { TableTemplate } from '../models/table-template.model';
import { SettingsGroup } from '../settings-group/settings-group';
import { StickyColumnsField } from '../sticky-columns-field/sticky-columns-field';
import { TemplateLayoutUtil } from '../template-layout.util';
import { TemplatesStore } from '../templates.store';

/**
 * Panel page for the open table: its name and, for a horizontal table, how many of its own columns
 * stay pinned. The orientation and version are in the toolbar, and its sections and column blocks
 * in the structure tree.
 */
@Component({
  selector: 'app-template-settings',
  imports: [ConfirmDeleteButton, InputTextModule, SettingsGroup, StickyColumnsField],
  templateUrl: './template-settings.html',
  styleUrl: './template-settings.scss',
})
export class TemplateSettings {
  public readonly template = computed<TableTemplate | null>(() => this.store.template());

  /** How many columns the table's own cells take, before the column blocks: the most that can be pinned. */
  public readonly ownColumns = computed(() => {
    const template = this.template();
    if (!template) {
      return 1;
    }
    const plan = TemplateLayoutUtil.planColumns(template);
    const firstBlock = Math.min(...plan.blockStarts.values(), plan.total + 1);
    return Math.max(1, firstBlock - 1);
  });

  private readonly store = inject(TemplatesStore);

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

  public setSticky(count: number): void {
    const template = this.template();
    if (template) {
      void this.store.updateTemplate(template.name, template.orientation, count);
    }
  }

  public delete(): void {
    void this.store.deleteTemplate();
  }
}
