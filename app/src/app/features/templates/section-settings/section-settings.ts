import { Component, computed, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { PanelRef } from '../models/panel-ref';
import { SectionEntry } from '../models/section-entry.model';
import { UpdateTemplateSectionRequest } from '../models/update-template-section-request.model';
import { MoveButtons } from '../move-buttons/move-buttons';
import { NumberField } from '../number-field/number-field';
import { PanelLinkList } from '../panel-link-list/panel-link-list';
import { PanelNavigator } from '../panel-navigator';
import { plural, sectionLinks } from '../section-links.util';
import { isHeader, sectionNoun } from '../section-role.util';
import { TemplatesStore } from '../templates.store';

type InstanceField = 'minInstances' | 'maxInstances' | 'initialInstances';

/**
 * Panel page for a section: its name, how it's added on a sheet, its own rows and its sub-sections. The
 * header is always exactly one and can't be moved or removed; every other section is addable.
 */
@Component({
  selector: 'app-section-settings',
  imports: [
    ButtonModule,
    ConfirmDeleteButton,
    InputTextModule,
    MoveButtons,
    NumberField,
    PanelLinkList,
  ],
  templateUrl: './section-settings.html',
  styleUrl: './section-settings.scss',
})
export class SectionSettings {
  public readonly sectionId = input.required<number>();

  public readonly entry = computed<SectionEntry | undefined>(() =>
    this.store.index().sections.get(this.sectionId()),
  );

  /** "Header", "Section" or "Sub-section". */
  public readonly noun = computed(() => {
    const section = this.entry()?.section;
    return section ? sectionNoun(section) : 'Section';
  });

  public readonly isHeader = computed(() => {
    const section = this.entry()?.section;
    return section ? isHeader(section) : false;
  });

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

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  /** Top-level sections are independent of each other, so they have no position to change. */
  public isTopLevel(): boolean {
    return this.entry()?.section.parentSectionId === null;
  }

  public position(): number {
    const entry = this.entry();
    return entry ? entry.siblings.indexOf(entry.section) + 1 : 1;
  }

  public async rename(input: HTMLInputElement): Promise<void> {
    const section = this.entry()?.section;
    const name = input.value.trim();
    if (section && name && name !== section.name) {
      await this.save({ name });
    }
    input.value = this.entry()?.section.name ?? '';
  }

  /** Saves one of the counts, nudging the others so fewest <= starts with <= most still holds. */
  public setCount(field: InstanceField, value: number | null): void {
    const section = this.entry()?.section;
    if (!section) {
      return;
    }
    const counts = {
      minInstances: section.minInstances,
      maxInstances: section.maxInstances,
      initialInstances: section.initialInstances,
      [field]: field === 'maxInstances' ? value : (value ?? 0),
    };
    if (field === 'minInstances') {
      counts.initialInstances = Math.max(counts.initialInstances, counts.minInstances);
    }
    if (counts.maxInstances !== null) {
      counts.initialInstances = Math.min(counts.initialInstances, counts.maxInstances);
      counts.minInstances = Math.min(counts.minInstances, counts.initialInstances);
    }
    void this.save(counts);
  }

  public move(position: number): void {
    void this.store.moveSection(this.sectionId(), position);
  }

  public async addSubSection(): Promise<void> {
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
    return 'Its rows and sub-sections, with their cells, go too.';
  }

  public async delete(): Promise<void> {
    const parent = this.entry()?.ancestors.at(-1);
    const fallback: PanelRef = parent ? { kind: 'section', id: parent.id } : { kind: 'template' };
    if (await this.store.deleteSection(this.sectionId())) {
      this.navigator.forgetMissing(fallback);
    }
  }

  private save(changes: Partial<UpdateTemplateSectionRequest>): Promise<void> {
    return this.store.updateSection(this.sectionId(), changes);
  }
}
