import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { SelectButtonChangeEvent, SelectButtonModule } from 'primeng/selectbutton';
import { TableTemplate } from '../models/table-template.model';
import { TemplateOrientation } from '../models/template-orientation';
import { TemplatesStore } from '../templates.store';

/**
 * The bar above the canvas: the table's name, which version is showing and its orientation, the
 * direction sections are added in (vertical: top to bottom; horizontal: left to right). A version
 * that can't change says so and offers the way on.
 */
@Component({
  selector: 'app-designer-toolbar',
  imports: [ButtonModule, FormsModule, SelectButtonModule, SelectModule],
  templateUrl: './designer-toolbar.html',
  styleUrl: './designer-toolbar.scss',
})
export class DesignerToolbar {
  public readonly template = input.required<TableTemplate>();

  public readonly orientations = [
    { label: 'Horizontal', value: 'Horizontal' },
    { label: 'Vertical', value: 'Vertical' },
  ];

  public readonly versionOptions = computed(() =>
    [...this.template().versions].reverse().map((version) => ({
      label: `Version ${version.versionNumber}${version.isInUse ? ' (in use)' : ''}`,
      value: version.versionNumber,
    })),
  );

  private readonly store = inject(TemplatesStore);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  /** Whether the version showing is the latest one. */
  public isLatest(): boolean {
    const template = this.template();
    return template.versionNumber === template.versions.at(-1)?.versionNumber;
  }

  public showVersion(event: SelectChangeEvent): void {
    this.store.showVersion(event.value as number);
  }

  public showLatest(): void {
    this.store.showVersion(null);
  }

  public createVersion(): void {
    void this.store.createVersion();
  }

  public setOrientation(event: SelectButtonChangeEvent): void {
    const orientation = event.value as TemplateOrientation | null;
    if (orientation && orientation !== this.template().orientation) {
      void this.store.updateTemplate(this.template().name, orientation);
    }
  }
}
