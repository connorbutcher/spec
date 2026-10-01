import { Component, computed, inject, input } from '@angular/core';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ButtonModule } from 'primeng/button';
import { SheetAddMenu } from '../sheet-add-menu/sheet-add-menu';
import { SheetRow } from '../models/sheet-row.model';
import { SheetSection } from '../models/sheet-section.model';
import { SheetTable } from '../models/sheet-table.model';
import { sectionAncestors, sectionSiblings } from '../sheet-index.util';
import { sectionLabel } from '../sheet-labels.util';
import { SheetStore } from '../sheet.store';

/**
 * The actions for what is selected in a table: where it is, the sections' and rows' own move and
 * remove actions (adding lives on the group that takes the new item). It sits above the grid so it works the same however the table is laid out.
 */
@Component({
  selector: 'app-sheet-action-bar',
  imports: [BreadcrumbModule, ButtonModule, SheetAddMenu],
  templateUrl: './sheet-action-bar.html',
  styleUrl: './sheet-action-bar.scss',
})
export class SheetActionBar {
  public readonly table = input.required<SheetTable>();

  public readonly section = computed<SheetSection | null>(() => {
    const selection = this.store.selection();
    if (selection?.tableId !== this.table().id || selection.sectionId === null) {
      return null;
    }
    return this.store.index().sections.get(selection.sectionId) ?? null;
  });

  public readonly row = computed<SheetRow | null>(() => {
    const selection = this.store.selection();
    if (selection?.tableId !== this.table().id || selection.rowId === null) {
      return null;
    }
    return this.store.index().rows.get(selection.rowId) ?? null;
  });

  /** Table › Group › Limits, each step selecting that section. */
  public readonly path = computed<MenuItem[]>(() => {
    const section = this.section();
    if (section === null) {
      return [];
    }
    const index = this.store.index();
    return [...sectionAncestors(index, section.id), section].map((step) => ({
      label: sectionLabel(step),
      command: () => this.store.selectSection(step.id),
    }));
  });

  public readonly isHeader = computed(() => this.section()?.role === 'Header');

  public readonly canMoveSectionUp = computed(() => {
    const section = this.section();
    if (section === null || this.isHeader()) {
      return false;
    }
    const siblings = sectionSiblings(this.store.index(), section.id);
    const first = siblings[0]?.role === 'Header' ? 1 : 0;
    return siblings.findIndex((candidate) => candidate.id === section.id) > first;
  });

  public readonly canMoveSectionDown = computed(() => {
    const section = this.section();
    if (section === null || this.isHeader()) {
      return false;
    }
    const siblings = sectionSiblings(this.store.index(), section.id);
    return siblings.findIndex((candidate) => candidate.id === section.id) < siblings.length - 1;
  });

  public readonly canMoveRowUp = computed(() => {
    const section = this.section();
    const row = this.row();
    return section !== null && row !== null && row.canRemove && section.rows.indexOf(row) > 0;
  });

  public readonly canMoveRowDown = computed(() => {
    const section = this.section();
    const row = this.row();
    return (
      section !== null &&
      row !== null &&
      row.canRemove &&
      section.rows.indexOf(row) < section.rows.length - 1
    );
  });

  public readonly lockedByOther = computed(() => {
    const lock = this.row()?.lock;
    return lock !== null && lock !== undefined && !lock.isMine;
  });

  public readonly isBusy = computed(() => this.store.isBusy());

  private readonly store = inject(SheetStore);
  private readonly confirmation = inject(ConfirmationService);

  public moveSection(step: -1 | 1): void {
    const section = this.section();
    if (section !== null) {
      void this.store.moveSection(section.id, step);
    }
  }

  public askRemoveSection(event: Event): void {
    const section = this.section();
    if (section === null) {
      return;
    }
    this.confirm(event, `Remove ${sectionLabel(section)}? Its rows and sections go too.`, () =>
      this.store.removeSection(section.id),
    );
  }

  public moveRow(step: -1 | 1): void {
    const row = this.row();
    if (row !== null) {
      void this.store.moveRow(row.id, step);
    }
  }

  public askRemoveRow(event: Event): void {
    const row = this.row();
    if (row !== null) {
      this.confirm(event, 'Remove this row?', () => this.store.removeRow(row.id));
    }
  }

  private confirm(event: Event, message: string, accept: () => Promise<void>): void {
    this.confirmation.confirm({
      target: event.currentTarget as EventTarget,
      message,
      acceptLabel: 'Remove',
      rejectLabel: 'Cancel',
      acceptButtonProps: { size: 'small', severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', size: 'small', outlined: true },
      accept: () => void accept(),
    });
  }
}
