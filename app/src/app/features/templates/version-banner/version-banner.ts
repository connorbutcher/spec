import { Component, computed, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TableTemplate } from '../models/table-template.model';
import { TemplatesStore } from '../templates.store';

/**
 * Explains why a version can't be edited and offers the way forward: open the latest version, or
 * start a new one when the latest is already used on sheets. Shows nothing for an editable version.
 */
@Component({
  selector: 'app-version-banner',
  imports: [ButtonModule, MessageModule],
  templateUrl: './version-banner.html',
  styleUrl: './version-banner.scss',
})
export class VersionBanner {
  public readonly template = input.required<TableTemplate>();

  public readonly latestNumber = computed(
    () => this.template().versions.at(-1)?.versionNumber ?? 1,
  );

  public readonly isLatest = computed(() => this.template().versionNumber === this.latestNumber());

  private readonly store = inject(TemplatesStore);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public showLatest(): void {
    this.store.showVersion(null);
  }

  public createVersion(): void {
    void this.store.createVersion();
  }
}
