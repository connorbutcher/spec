import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { SelectButtonChangeEvent, SelectButtonModule } from 'primeng/selectbutton';
import { TableTemplate } from '../models/table-template.model';
import { TemplateOrientation } from '../models/template-orientation';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

/**
 * The bar above the canvas: the table's name, which version is showing and its orientation: the
 * direction sections are added in, top to bottom (vertical) or left to right (horizontal).
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
    {
      label: 'Horizontal',
      value: 'Horizontal',
      icon: 'pi pi-arrows-h',
      hint: 'Sections are added left to right',
    },
    {
      label: 'Vertical',
      value: 'Vertical',
      icon: 'pi pi-arrows-v',
      hint: 'Sections are added top to bottom',
    },
  ];

  public readonly versionOptions = computed(() =>
    [...this.template().versions].reverse().map((version) => ({
      label: `Version ${version.versionNumber}${version.isInUse ? ' (in use)' : ''}`,
      value: version.versionNumber,
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

  public isTemplateOpen(): boolean {
    return this.navigator.current().kind === 'template';
  }

  public openSettings(): void {
    this.navigator.open({ kind: 'template' });
  }

  public showVersion(event: SelectChangeEvent): void {
    this.store.showVersion(event.value as number);
  }

  public setOrientation(event: SelectButtonChangeEvent): void {
    const orientation = event.value as TemplateOrientation | null;
    if (orientation && orientation !== this.template().orientation) {
      void this.store.updateTemplate(this.template().name, orientation);
    }
  }
}
