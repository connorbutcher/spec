import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { SheetTypeIcon } from '../../../shared/sheet-type-icon';
import { TemplateListGroup } from '../models/template-list-group.model';
import { TemplatesStore } from '../templates.store';

/**
 * The open table's name in the toolbar, as a PrimeNG select of every table grouped by sheet type, so
 * switching tables doesn't need the list.
 */
@Component({
  selector: 'app-template-picker',
  imports: [FormsModule, SelectModule],
  templateUrl: './template-picker.html',
  styleUrl: './template-picker.scss',
})
export class TemplatePicker {
  public readonly groups = computed<TemplateListGroup[]>(() =>
    this.store
      .sheetTypes()
      .map((sheetType) => ({
        sheetTypeId: sheetType.id,
        label: sheetType.name,
        icon: SheetTypeIcon.sheetTypeIcon(sheetType.name),
        items: this.store.templatesFor(sheetType.id),
      }))
      .filter((group) => group.items.length > 0),
  );

  private readonly store = inject(TemplatesStore);
  private readonly router = inject(Router);

  public selectedTemplateId(): number | null {
    return this.store.selectedTemplateId();
  }

  public open(event: SelectChangeEvent): void {
    const id = event.value as number | null;
    if (id !== null && id !== this.store.selectedTemplateId()) {
      void this.router.navigate(['/templates', id]);
    }
  }
}
