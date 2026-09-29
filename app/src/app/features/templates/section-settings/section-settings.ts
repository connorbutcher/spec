import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { SelectButtonChangeEvent, SelectButtonModule } from 'primeng/selectbutton';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { FixedPresence } from '../models/fixed-presence';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { PanelRef } from '../models/panel-ref';
import { SectionEntry } from '../models/section-entry.model';
import { SectionRole } from '../models/section-role';
import { UpdateTemplateSectionRequest } from '../models/update-template-section-request.model';
import { MoveButtons } from '../move-buttons/move-buttons';
import { NumberField } from '../number-field/number-field';
import { PanelLinkList } from '../panel-link-list/panel-link-list';
import { PanelNavigator } from '../panel-navigator';
import { plural, sectionLinks } from '../section-links.util';
import { countsForRole, fixedCounts, fixedPresence } from '../section-role.util';
import { TemplatesStore } from '../templates.store';

type InstanceField = 'minInstances' | 'maxInstances' | 'initialInstances';

/**
 * Panel page for a section: its name, position, role and either the sections inside it or its rows. A
 * section holds one or the other, so an empty section offers both.
 */
@Component({
  selector: 'app-section-settings',
  imports: [
    ButtonModule,
    ConfirmDeleteButton,
    FormsModule,
    InputTextModule,
    MoveButtons,
    NumberField,
    PanelLinkList,
    SelectButtonModule,
    SelectModule,
  ],
  templateUrl: './section-settings.html',
  styleUrl: './section-settings.scss',
})
export class SectionSettings {
  public readonly sectionId = input.required<number>();

  public readonly roles = [
    { label: 'Fixed', value: 'Fixed' },
    { label: 'Repeating', value: 'Repeating' },
  ];

  public readonly presences = [
    { label: 'Always included (e.g. a header)', value: 'always' },
    { label: 'Included, can be removed', value: 'default' },
    { label: 'Optional, added when needed', value: 'optional' },
  ];

  public readonly entry = computed<SectionEntry | undefined>(() =>
    this.store.index().sections.get(this.sectionId()),
  );

  public readonly presence = computed<FixedPresence | null>(() => {
    const section = this.entry()?.section;
    return section ? fixedPresence(section) : null;
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
      await this.save({ name });
    }
    input.value = this.entry()?.section.name ?? '';
  }

  public setRole(event: SelectButtonChangeEvent): void {
    const section = this.entry()?.section;
    const role = event.value as SectionRole | null;
    if (section && role && role !== section.role) {
      void this.save(countsForRole(section, role));
    }
  }

  public setPresence(event: SelectChangeEvent): void {
    void this.save(fixedCounts(event.value as FixedPresence));
  }

  /** Saves one of a repeating section's counts, nudging the others so min <= start <= max holds. */
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

  private save(changes: Partial<UpdateTemplateSectionRequest>): Promise<void> {
    return this.store.updateSection(this.sectionId(), changes);
  }
}
