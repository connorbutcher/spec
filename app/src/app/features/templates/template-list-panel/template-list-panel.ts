import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ListboxChangeEvent, ListboxModule } from 'primeng/listbox';
import { TooltipModule } from 'primeng/tooltip';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { sheetTypeIcon } from '../../../shared/sheet-type-icon';
import { TemplateListGroup } from '../models/template-list-group.model';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

/**
 * The left-hand card: one PrimeNG list grouped by sheet type, each group with an add-table button,
 * and a way into the cell types.
 */
@Component({
  selector: 'app-template-list-panel',
  imports: [ButtonModule, EmptyState, FormsModule, ListboxModule, TooltipModule],
  templateUrl: './template-list-panel.html',
  styleUrl: './template-list-panel.scss',
})
export class TemplateListPanel {
  public readonly groups = computed<TemplateListGroup[]>(() =>
    this.store.sheetTypes().map((sheetType) => ({
      sheetTypeId: sheetType.id,
      label: sheetType.name,
      icon: sheetTypeIcon(sheetType.name),
      items: this.store.templatesFor(sheetType.id),
    })),
  );

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);
  private readonly router = inject(Router);

  public selectedTemplateId(): number | null {
    return this.store.selectedTemplateId();
  }

  public isLoading(): boolean {
    return this.store.listIsLoading() && this.store.sheetTypes().length === 0;
  }

  public hasError(): boolean {
    return this.store.listHasError();
  }

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public cellTypeCount(): number {
    return this.store.cellTypes().length;
  }

  public isShowingCellTypes(): boolean {
    const current = this.navigator.current();
    return current.kind === 'cellTypes' || current.kind === 'cellType';
  }

  public open(event: ListboxChangeEvent): void {
    const id = event.value as number | null;
    if (id !== null && id !== this.store.selectedTemplateId()) {
      void this.router.navigate(['/templates', id]);
    }
  }

  public add(group: TemplateListGroup, event: Event): void {
    // The button sits in the list's group header; keep the click from reaching the list.
    event.stopPropagation();
    void this.store.createTemplate(group.sheetTypeId);
  }

  public openCellTypes(): void {
    this.navigator.open({ kind: 'cellTypes' });
  }

  public retry(): void {
    this.store.reloadList();
  }
}
