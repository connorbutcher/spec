import { Component, computed, inject, input } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { InstanceCountsFields } from '../instance-counts-fields/instance-counts-fields';
import { InstanceCounts } from '../models/instance-counts.model';
import { SectionEntry } from '../models/section-entry.model';
import { UpdateTemplateSectionRequest } from '../models/update-template-section-request.model';
import { SectionRoleUtil } from '../section-role.util';
import { SettingsGroup } from '../settings-group/settings-group';
import { TemplatesStore } from '../templates.store';

/**
 * Panel page for a section: its name and how it's added on a sheet. The header is always exactly one.
 * Its rows and sub-sections are in the structure tree, and adding, moving and deleting are in the
 * selection bar.
 */
@Component({
  selector: 'app-section-settings',
  imports: [InputTextModule, InstanceCountsFields, SettingsGroup],
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
    return section ? SectionRoleUtil.sectionNoun(section) : 'Section';
  });

  public readonly isHeader = computed(() => {
    const section = this.entry()?.section;
    return section ? SectionRoleUtil.isHeader(section) : false;
  });

  private readonly store = inject(TemplatesStore);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  public async rename(input: HTMLInputElement): Promise<void> {
    const section = this.entry()?.section;
    const name = input.value.trim();
    if (section && name && name !== section.name) {
      await this.save({ name });
    }
    input.value = this.entry()?.section.name ?? '';
  }

  public setCounts(counts: InstanceCounts): void {
    void this.save(counts);
  }

  private save(changes: Partial<UpdateTemplateSectionRequest>): Promise<void> {
    return this.store.updateSection(this.sectionId(), changes);
  }
}
