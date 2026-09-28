import { Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SheetType } from '../../../core/models/sheet-type.model';
import { sheetTypeIcon } from '../../../shared/sheet-type-icon';
import { TableTemplateSummary } from '../models/table-template-summary.model';
import { TemplatesStore } from '../templates.store';

/** One sheet type in the list: its name, an add-table button and its tables. */
@Component({
  selector: 'app-template-list-group',
  imports: [RouterLink],
  templateUrl: './template-list-group.html',
  styleUrl: './template-list-group.scss',
})
export class TemplateListGroup {
  public readonly sheetType = input.required<SheetType>();

  private readonly store = inject(TemplatesStore);

  public icon(): string {
    return sheetTypeIcon(this.sheetType().name);
  }

  public templates(): TableTemplateSummary[] {
    return this.store.templatesFor(this.sheetType().id);
  }

  public isSelected(template: TableTemplateSummary): boolean {
    return this.store.selectedTemplateId() === template.id;
  }

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public add(): void {
    void this.store.createTemplate(this.sheetType().id);
  }
}
