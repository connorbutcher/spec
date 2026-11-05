import { inject, Injectable, signal } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { ContextMenu } from 'primeng/contextmenu';
import { PanelRef } from './models/panel-ref';
import { StructureAction } from './models/structure-action.model';
import { PanelNavigator } from './panel-navigator';
import { SectionRoleUtil } from './section-role.util';
import { StructureActions } from './structure-actions';
import { StructureIcons } from './structure-icons';
import { TemplatesStore } from './templates.store';

/**
 * The one pop-up menu for a part of the table, opened by right-clicking it in the preview or the tree,
 * or with the tree's "⋯" button. It selects what it was opened on, then lists its settings, its
 * actions and, last, its delete. The browser also fires the context-menu event for the keyboard menu
 * key and Shift+F10, so it works from the keyboard too. Provided by the designer, which registers the
 * PrimeNG context menu it shows.
 */
@Injectable()
export class StructureMenu {
  public readonly items = signal<MenuItem[]>([]);

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);
  private readonly actions = inject(StructureActions);

  private menu: ContextMenu | undefined;
  private anchor: EventTarget | null = null;

  public register(menu: ContextMenu): void {
    this.menu = menu;
  }

  /** Selects `ref` and shows its menu at the pointer (or under the element, from the keyboard). */
  public open(event: MouseEvent, ref: PanelRef): void {
    event.preventDefault();
    event.stopPropagation();
    if (ref.kind !== 'template') {
      this.navigator.open(ref);
    }
    this.anchor = event.currentTarget;
    this.items.set(ref.kind === 'cell' ? this.cellItems(ref.id) : this.itemsFor(ref));
    this.menu?.show(event);
  }

  /** Settings, then the actions, then the delete apart at the end. */
  private itemsFor(ref: PanelRef): MenuItem[] {
    const actions = this.actions.actionsFor(ref);
    const plain = actions.filter((action) => !action.danger);
    const danger = actions.filter((action) => action.danger);
    return [
      this.settingsItem(ref),
      ...(plain.length > 0
        ? [{ separator: true }, ...plain.map((action) => this.item(action))]
        : []),
      ...(danger.length > 0
        ? [{ separator: true }, ...danger.map((action) => this.item(action))]
        : []),
    ];
  }

  /**
   * A cell's menu also reaches its row, column block and section, since the cell is what's under the
   * pointer in the preview.
   */
  private cellItems(cellId: number): MenuItem[] {
    const index = this.store.index();
    const entry = index.cells.get(cellId);
    if (!entry) {
      return [];
    }
    const cellRef: PanelRef = { kind: 'cell', id: cellId };
    const rowRef: PanelRef = { kind: 'row', id: entry.row.row.id };
    const sectionRef: PanelRef = { kind: 'section', id: entry.row.section.id };
    const blockId = entry.cell.columnBlockId;
    const cell = this.actions.actionsFor(cellRef);
    const row = this.actions.actionsFor(rowRef);
    return [
      this.settingsItem(cellRef),
      this.settingsItem(rowRef),
      ...(blockId === null ? [] : [this.settingsItem({ kind: 'columnBlock', id: blockId })]),
      { separator: true },
      ...this.pick(row, ['insertAbove', 'insertBelow']),
      ...this.pick(cell, ['insertLeft', 'insertRight']),
      { separator: true },
      {
        label: `${SectionRoleUtil.sectionNoun(entry.row.section)}: ${entry.row.section.name}`,
        icon: `pi ${StructureIcons.icon(StructureIcons.levelOf(sectionRef, index))}`,
        items: this.itemsFor(sectionRef),
      },
      { separator: true },
      ...this.pick(cell, ['delete']),
      ...row.filter((action) => action.danger).map((action) => this.item(action)),
    ];
  }

  private settingsItem(ref: PanelRef): MenuItem {
    const index = this.store.index();
    const level = StructureIcons.levelOf(ref, index);
    const row = ref.kind === 'row' ? index.rows.get(ref.id) : undefined;
    const block = ref.kind === 'columnBlock' ? index.columnBlocks.get(ref.id) : undefined;
    const name = row ? `Row ${row.number}` : (block?.block.name ?? StructureIcons.noun(level));
    return {
      label: `${name} settings`,
      icon: `pi ${StructureIcons.icon(level)}`,
      command: () => this.navigator.open(ref),
    };
  }

  /** The menu items for the actions in `list` with one of `ids`. */
  private pick(list: StructureAction[], ids: string[]): MenuItem[] {
    return list.filter((action) => ids.includes(action.id)).map((action) => this.item(action));
  }

  private item(action: StructureAction): MenuItem {
    return {
      label: action.label,
      icon: `pi ${action.icon}`,
      disabled: action.disabled,
      styleClass: action.danger ? 'danger' : undefined,
      command: () => action.run(this.anchor),
    };
  }
}
