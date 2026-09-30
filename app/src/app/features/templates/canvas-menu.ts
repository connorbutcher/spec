import { inject, Injectable, signal } from '@angular/core';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { ContextMenu } from 'primeng/contextmenu';
import { CellEntry } from './models/cell-entry.model';
import { PanelRef } from './models/panel-ref';
import { SectionEntry } from './models/section-entry.model';
import { PanelNavigator } from './panel-navigator';
import { isHeader, sectionNoun } from './section-role.util';
import { TemplatesStore } from './templates.store';

/**
 * The canvas's right-click menu: builds the items for whatever was clicked (a cell, an empty section
 * or the empty canvas) and shows the PrimeNG context menu the canvas registers. The browser also fires
 * the context-menu event for the keyboard menu key and Shift+F10, so this works from the keyboard too.
 * Provided by the template canvas.
 */
@Injectable()
export class CanvasMenu {
  public readonly items = signal<MenuItem[]>([]);

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);
  private readonly confirmation = inject(ConfirmationService);

  private menu: ContextMenu | undefined;
  private anchor: EventTarget | null = null;

  public register(menu: ContextMenu): void {
    this.menu = menu;
  }

  public openForCell(event: MouseEvent, cellId: number): void {
    const entry = this.store.index().cells.get(cellId);
    if (entry) {
      this.navigator.open({ kind: 'cell', id: cellId });
      this.show(event, this.cellItems(entry));
    }
  }

  public openForSection(event: MouseEvent, sectionId: number): void {
    const entry = this.store.index().sections.get(sectionId);
    if (entry) {
      this.navigator.open({ kind: 'section', id: sectionId });
      this.show(event, this.sectionItems(entry));
    }
  }

  public openForCanvas(event: MouseEvent): void {
    this.show(event, [
      {
        label: 'Table settings',
        icon: 'pi pi-cog',
        command: () => this.navigator.open({ kind: 'template' }),
      },
      { separator: true },
      {
        label: 'Add section',
        icon: 'pi pi-plus',
        disabled: !this.store.canEdit(),
        command: () => void this.addSection(null),
      },
    ]);
  }

  private show(event: MouseEvent, items: MenuItem[]): void {
    event.preventDefault();
    event.stopPropagation();
    this.anchor = event.currentTarget;
    this.items.set(items);
    this.menu?.show(event);
  }

  private cellItems(entry: CellEntry): MenuItem[] {
    const { cell, row } = entry;
    const locked = !this.store.canEdit();
    const section = this.store.index().sections.get(row.section.id);

    return [
      {
        label: 'Cell settings',
        icon: 'pi pi-cog',
        command: () => this.open({ kind: 'cell', id: cell.id }),
      },
      {
        label: `Row ${row.number} settings`,
        icon: 'pi pi-bars',
        command: () => this.open({ kind: 'row', id: row.row.id }),
      },
      { separator: true },
      // Rows always run across the page and cells sit side by side in them, whatever the orientation.
      {
        label: 'Insert row above',
        icon: 'pi pi-arrow-up',
        disabled: locked,
        command: () => void this.addRow(row.section.id, row.number, row.row.id),
      },
      {
        label: 'Insert row below',
        icon: 'pi pi-arrow-down',
        disabled: locked,
        command: () => void this.addRow(row.section.id, row.number + 1, row.row.id),
      },
      {
        label: 'Insert cell to the left',
        icon: 'pi pi-arrow-left',
        disabled: locked,
        command: () => void this.addCell(row.row.id, cell.column),
      },
      {
        label: 'Insert cell to the right',
        icon: 'pi pi-arrow-right',
        disabled: locked,
        command: () => void this.addCell(row.row.id, cell.column + cell.columnSpan),
      },
      { separator: true },
      ...(section
        ? [
            {
              label: `${sectionNoun(section.section)}: ${section.section.name}`,
              icon: 'pi pi-objects-column',
              items: this.sectionItems(section),
            },
          ]
        : []),
      { separator: true },
      {
        label: 'Delete cell',
        icon: 'pi pi-trash',
        disabled: locked,
        command: () =>
          this.confirmDelete('Delete this cell?', async () => {
            if (await this.store.deleteCell(cell.id)) {
              this.navigator.forgetMissing({ kind: 'row', id: row.row.id });
            }
          }),
      },
      {
        label: `Delete row ${row.number}`,
        icon: 'pi pi-trash',
        disabled: locked,
        command: () =>
          this.confirmDelete(`Delete row ${row.number} and its cells?`, async () => {
            if (await this.store.deleteRow(row.row.id)) {
              this.navigator.forgetMissing({ kind: 'section', id: row.section.id });
            }
          }),
      },
    ];
  }

  private sectionItems(entry: SectionEntry): MenuItem[] {
    const { section } = entry;
    const noun = sectionNoun(section);
    const header = isHeader(section);
    const locked = !this.store.canEdit();
    const parentId = entry.ancestors.at(-1)?.id ?? null;

    return [
      {
        label: `${noun} settings`,
        icon: 'pi pi-cog',
        command: () => this.open({ kind: 'section', id: section.id }),
      },
      { separator: true },
      {
        label: 'Add row',
        icon: 'pi pi-plus',
        disabled: locked,
        command: () => void this.addRow(section.id, null, null),
      },
      {
        label: 'Add sub-section',
        icon: 'pi pi-sitemap',
        // The header holds rows only.
        disabled: locked || header,
        command: () => void this.addSection(section.id),
      },
      {
        // A sub-section's sibling goes in its parent; at the top level a new section is an addable one.
        label: parentId === null ? 'Add section' : 'Add sub-section alongside',
        icon: 'pi pi-plus-circle',
        disabled: locked,
        command: () => void this.addSection(parentId),
      },
      { separator: true },
      {
        label: `Delete ${noun.toLowerCase()}`,
        icon: 'pi pi-trash',
        // The header is part of every table.
        disabled: locked || header,
        command: () =>
          this.confirmDelete(`Delete "${section.name}" with everything in it?`, async () => {
            const fallback: PanelRef =
              parentId === null ? { kind: 'template' } : { kind: 'section', id: parentId };
            if (await this.store.deleteSection(section.id)) {
              this.navigator.forgetMissing(fallback);
            }
          }),
      },
    ];
  }

  private open(ref: PanelRef): void {
    this.navigator.open(ref);
  }

  private async addRow(
    sectionId: number,
    position: number | null,
    copyFromRowId: number | null,
  ): Promise<void> {
    const id = await this.store.addRow(sectionId, position, copyFromRowId);
    if (id !== null) {
      this.navigator.open({ kind: 'row', id });
    }
  }

  private async addCell(rowId: number, column: number): Promise<void> {
    const id = await this.store.addCell(rowId, column);
    if (id !== null) {
      this.navigator.open({ kind: 'cell', id });
    }
  }

  private async addSection(parentId: number | null): Promise<void> {
    const id = await this.store.addSection(parentId);
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }

  /** Asks in a confirm popup next to what was right-clicked before deleting. */
  private confirmDelete(message: string, accept: () => Promise<void>): void {
    this.confirmation.confirm({
      target: this.anchor ?? undefined,
      message,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonProps: { severity: 'danger', size: 'small' },
      rejectButtonProps: { severity: 'secondary', size: 'small', outlined: true },
      accept: () => void accept(),
    });
  }
}
