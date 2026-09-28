import { Component, computed, inject, input } from '@angular/core';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { PanelRef } from '../models/panel-ref';
import { SectionEntry } from '../models/section-entry.model';
import { MoveButtons } from '../move-buttons/move-buttons';
import { PanelLinkList } from '../panel-link-list/panel-link-list';
import { PanelNavigator } from '../panel-navigator';
import { plural, sectionLinks } from '../section-links.util';
import { TemplatesStore } from '../templates.store';

/**
 * Panel page for a section: its name, position, and either the sections inside it or its rows. A
 * section holds one or the other, so an empty section offers both.
 */
@Component({
  selector: 'app-section-settings',
  imports: [ConfirmDeleteButton, MoveButtons, PanelLinkList],
  templateUrl: './section-settings.html',
  styleUrl: './section-settings.scss',
})
export class SectionSettings {
  public readonly sectionId = input.required<number>();

  public readonly entry = computed<SectionEntry | undefined>(() =>
    this.store.index().sections.get(this.sectionId()),
  );

  public readonly childSections = computed<PanelLinkItem[]>(() =>
    sectionLinks(this.entry()?.section.sections ?? []),
  );

  public readonly rows = computed<PanelLinkItem[]>(() =>
    (this.entry()?.section.rows ?? []).map((row, index) => ({
      ref: { kind: 'row', id: row.id },
      label: `Row ${index + 1}`,
      icon: 'pi-bars',
      meta: plural(row.cells.length, 'cell'),
      muted: row.cells.length === 0,
    })),
  );

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public hasChildSections(): boolean {
    return (this.entry()?.section.sections.length ?? 0) > 0;
  }

  public hasRows(): boolean {
    return (this.entry()?.section.rows.length ?? 0) > 0;
  }

  public position(): number {
    const entry = this.entry();
    return entry ? entry.siblings.indexOf(entry.section) + 1 : 1;
  }

  public async rename(input: HTMLInputElement): Promise<void> {
    const section = this.entry()?.section;
    const name = input.value.trim();
    if (section && name && name !== section.name) {
      await this.store.renameSection(section.id, name);
    }
    input.value = this.entry()?.section.name ?? '';
  }

  public move(position: number): void {
    void this.store.moveSection(this.sectionId(), position);
  }

  public async addSection(): Promise<void> {
    const id = await this.store.addSection(this.sectionId());
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }

  public async addRow(): Promise<void> {
    const id = await this.store.addRow(this.sectionId());
    if (id !== null) {
      this.navigator.open({ kind: 'row', id });
    }
  }

  public deleteWarning(): string {
    return this.hasChildSections()
      ? 'The sections inside it, with their rows and cells, go too.'
      : 'Its rows and cells go too.';
  }

  public async delete(): Promise<void> {
    const parent = this.entry()?.ancestors.at(-1);
    const fallback: PanelRef = parent ? { kind: 'section', id: parent.id } : { kind: 'template' };
    if (await this.store.deleteSection(this.sectionId())) {
      this.navigator.forgetMissing(fallback);
    }
  }
}
