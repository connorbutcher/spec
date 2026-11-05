import { inject, Injectable } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import { CellEntry } from './models/cell-entry.model';
import { ColumnBlockEntry } from './models/column-block-entry.model';
import { PanelRef } from './models/panel-ref';
import { RowEntry } from './models/row-entry.model';
import { SectionEntry } from './models/section-entry.model';
import { StructureAction } from './models/structure-action.model';
import { PanelNavigator } from './panel-navigator';
import { SectionRoleUtil } from './section-role.util';
import { TemplatesStore } from './templates.store';

/**
 * Everything that adds, moves or removes part of a table, in one place: the selection bar, the tree's
 * "+" and "⋯" buttons, the right-click menu and the keyboard shortcuts all use these, so they always
 * offer the same things with the same names. Each action opens what it adds, and after a delete the
 * panel falls back to the parent. Provided by the templates page.
 */
@Injectable()
export class StructureActions {
  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);
  private readonly confirmation = inject(ConfirmationService);

  /** The actions for a part of the table, in the order they're shown. Empty for the cell types. */
  public actionsFor(ref: PanelRef): StructureAction[] {
    const index = this.store.index();
    switch (ref.kind) {
      case 'template':
        return this.tableActions();
      case 'section': {
        const entry = index.sections.get(ref.id);
        return entry ? this.sectionActions(entry) : [];
      }
      case 'row': {
        const entry = index.rows.get(ref.id);
        return entry ? this.rowActions(entry) : [];
      }
      case 'cell': {
        const entry = index.cells.get(ref.id);
        return entry ? this.cellActions(entry) : [];
      }
      case 'columnBlock': {
        const entry = index.columnBlocks.get(ref.id);
        return entry ? this.columnBlockActions(entry) : [];
      }
      default:
        return [];
    }
  }

  /**
   * The one most likely addition for a part of the table, for the tree's "+": a section in the table,
   * a row in a section, a cell in a row. Null when there's nothing to add (a cell or column block).
   */
  public addChildFor(ref: PanelRef): StructureAction | null {
    const id =
      ref.kind === 'template' ? 'addSection' : ref.kind === 'section' ? 'addRow' : 'addCell';
    return ref.kind === 'template' || ref.kind === 'section' || ref.kind === 'row'
      ? (this.actionsFor(ref).find((action) => action.id === id) ?? null)
      : null;
  }

  /** The column block group's "+". */
  public addColumnBlock(): StructureAction {
    return this.action('addColumnBlock', 'Add column block', 'pi-plus', () => {
      void this.open(this.store.addColumnBlock(), (id) => ({ kind: 'columnBlock', id }));
    });
  }

  /** Moves a sub-section, row or column block one place up or down, if it can go there. */
  public move(ref: PanelRef, direction: -1 | 1): void {
    const action = this.actionsFor(ref).find(
      (candidate) => candidate.id === (direction < 0 ? 'moveUp' : 'moveDown'),
    );
    if (action && !action.disabled) {
      action.run(null);
    }
  }

  /** Moves a sub-section, row or column block to a 1-based position among its siblings. */
  public async moveTo(ref: PanelRef, position: number): Promise<void> {
    if (!this.store.canEdit() || this.store.isSaving()) {
      return;
    }
    if (ref.kind === 'section') {
      await this.store.moveSection(ref.id, position);
    } else if (ref.kind === 'row') {
      await this.store.moveRow(ref.id, position);
    } else if (ref.kind === 'columnBlock') {
      await this.store.moveColumnBlock(ref.id, position);
    }
  }

  private tableActions(): StructureAction[] {
    const horizontal = this.store.template()?.orientation === 'Horizontal';
    return [
      this.action('addSection', 'Add section', 'pi-plus', () => {
        void this.open(this.store.addSection(null), (id) => ({ kind: 'section', id }));
      }),
      ...(horizontal ? [this.addColumnBlock()] : []),
    ];
  }

  private sectionActions(entry: SectionEntry): StructureAction[] {
    const { section } = entry;
    const header = SectionRoleUtil.isHeader(section);
    const noun = SectionRoleUtil.sectionNoun(section).toLowerCase();
    const parentId = entry.ancestors.at(-1)?.id ?? null;
    const position = entry.siblings.indexOf(section) + 1;
    // Only sub-sections have an order: the header is always first and top-level sections stand alone.
    const movable = parentId !== null;
    const fallback: PanelRef =
      parentId === null ? { kind: 'template' } : { kind: 'section', id: parentId };

    return [
      this.action('addRow', 'Add row', 'pi-plus', () => {
        void this.open(this.store.addRow(section.id), (id) => ({ kind: 'row', id }));
      }),
      // The header holds rows only.
      ...(header
        ? []
        : [
            this.action('addSubSection', 'Add sub-section', 'pi-sitemap', () => {
              void this.open(this.store.addSection(section.id), (id) => ({ kind: 'section', id }));
            }),
          ]),
      ...(movable
        ? [
            this.moveAction(
              'moveUp',
              'Move up',
              'pi-arrow-up',
              position > 1,
              () => void this.moveTo({ kind: 'section', id: section.id }, position - 1),
            ),
            this.moveAction(
              'moveDown',
              'Move down',
              'pi-arrow-down',
              position < entry.siblings.length,
              () => void this.moveTo({ kind: 'section', id: section.id }, position + 1),
            ),
          ]
        : []),
      // The header is part of every table.
      ...(header
        ? []
        : [
            this.deleteAction(
              `Delete ${noun}`,
              `Delete "${section.name}" with everything in it?`,
              async () => {
                if (await this.store.deleteSection(section.id)) {
                  this.navigator.forgetMissing(fallback);
                }
              },
            ),
          ]),
    ];
  }

  private rowActions(entry: RowEntry): StructureAction[] {
    const { row, section, number } = entry;
    const blocks = [...this.store.index().columnBlocks.values()].sort(
      (a, b) => a.number - b.number,
    );
    const ref: PanelRef = { kind: 'row', id: row.id };

    return [
      this.action('addCell', 'Add cell', 'pi-plus', () => {
        void this.open(this.store.addCell(row.id), (id) => ({ kind: 'cell', id }));
      }),
      ...blocks.map(({ block }) =>
        this.action(`addCell-${block.id}`, `Add cell to ${block.name}`, 'pi-plus', () => {
          void this.open(this.store.addCell(row.id, null, block.id), (id) => ({
            kind: 'cell',
            id,
          }));
        }),
      ),
      // A new row copies this row's columns.
      this.insertAction('insertAbove', 'Insert row above', 'pi-angle-double-up', () => {
        void this.open(this.store.addRow(section.id, number, row.id), (id) => ({
          kind: 'row',
          id,
        }));
      }),
      this.insertAction('insertBelow', 'Insert row below', 'pi-angle-double-down', () => {
        void this.open(this.store.addRow(section.id, number + 1, row.id), (id) => ({
          kind: 'row',
          id,
        }));
      }),
      this.moveAction(
        'moveUp',
        'Move up',
        'pi-arrow-up',
        number > 1,
        () => void this.moveTo(ref, number - 1),
      ),
      this.moveAction(
        'moveDown',
        'Move down',
        'pi-arrow-down',
        number < section.rows.length,
        () => void this.moveTo(ref, number + 1),
      ),
      this.deleteAction('Delete row', `Delete row ${number} and its cells?`, async () => {
        if (await this.store.deleteRow(row.id)) {
          this.navigator.forgetMissing({ kind: 'section', id: section.id });
        }
      }),
    ];
  }

  private cellActions(entry: CellEntry): StructureAction[] {
    const { cell, row } = entry;
    // Rows always run across the page and cells sit side by side in them, whatever the orientation.
    return [
      this.insertAction('insertLeft', 'Insert cell to the left', 'pi-angle-double-left', () => {
        void this.open(this.store.addCell(row.row.id, cell.column, cell.columnBlockId), (id) => ({
          kind: 'cell',
          id,
        }));
      }),
      this.insertAction('insertRight', 'Insert cell to the right', 'pi-angle-double-right', () => {
        void this.open(
          this.store.addCell(row.row.id, cell.column + cell.columnSpan, cell.columnBlockId),
          (id) => ({ kind: 'cell', id }),
        );
      }),
      this.deleteAction('Delete cell', 'Delete this cell?', async () => {
        if (await this.store.deleteCell(cell.id)) {
          this.navigator.forgetMissing({ kind: 'row', id: row.row.id });
        }
      }),
    ];
  }

  private columnBlockActions(entry: ColumnBlockEntry): StructureAction[] {
    const { block, number } = entry;
    const count = this.store.index().columnBlocks.size;
    const ref: PanelRef = { kind: 'columnBlock', id: block.id };

    return [
      this.moveAction(
        'moveUp',
        'Move left',
        'pi-arrow-left',
        number > 1,
        () => void this.moveTo(ref, number - 1),
      ),
      this.moveAction(
        'moveDown',
        'Move right',
        'pi-arrow-right',
        number < count,
        () => void this.moveTo(ref, number + 1),
      ),
      this.deleteAction(
        'Delete column block',
        `Delete "${block.name}"? Its cells in every row go too.`,
        async () => {
          if (await this.store.deleteColumnBlock(block.id)) {
            this.navigator.forgetMissing({ kind: 'template' });
          }
        },
      ),
    ];
  }

  private action(id: string, label: string, icon: string, run: () => void): StructureAction {
    return { id, label, icon, disabled: this.isLocked(), danger: false, iconOnly: false, run };
  }

  private insertAction(id: string, label: string, icon: string, run: () => void): StructureAction {
    return { ...this.action(id, label, icon, run), iconOnly: true };
  }

  private moveAction(
    id: string,
    label: string,
    icon: string,
    possible: boolean,
    run: () => void,
  ): StructureAction {
    return { ...this.insertAction(id, label, icon, run), disabled: this.isLocked() || !possible };
  }

  /** A delete that asks first, in a confirm popup next to whatever it was started from. */
  private deleteAction(
    label: string,
    question: string,
    remove: () => Promise<void>,
  ): StructureAction {
    return {
      id: 'delete',
      label,
      icon: 'pi-trash',
      disabled: this.isLocked(),
      danger: true,
      iconOnly: true,
      run: (target) =>
        this.confirmation.confirm({
          target: target ?? undefined,
          message: question,
          icon: 'pi pi-exclamation-triangle',
          acceptLabel: 'Delete',
          rejectLabel: 'Cancel',
          acceptButtonProps: { severity: 'danger', size: 'small' },
          rejectButtonProps: { severity: 'secondary', size: 'small', outlined: true },
          accept: () => void remove(),
        }),
    };
  }

  private isLocked(): boolean {
    return !this.store.canEdit() || this.store.isSaving();
  }

  /** Waits for an add and opens what it made. */
  private async open(added: Promise<number | null>, ref: (id: number) => PanelRef): Promise<void> {
    const id = await added;
    if (id !== null) {
      this.navigator.open(ref(id));
    }
  }
}
