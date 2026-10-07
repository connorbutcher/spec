import { Component, computed, inject, input, viewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { Menu, MenuModule } from 'primeng/menu';
import { AddChoice } from '../models/add-choice.model';
import { AddableColumnBlock } from '../models/addable-column-block.model';
import { AddableSection } from '../models/addable-section.model';
import { SheetStore } from '../sheet.store';

/**
 * The one add button for a place that can grow: a table or a group section. It offers the section
 * types the place takes and, for a horizontal table, its column blocks. When a single thing can be
 * added the button adds it; when there is a choice it opens a menu of what can go here. Sections at
 * their template maximum are shown but can't be picked.
 */
@Component({
  selector: 'app-sheet-add-menu',
  imports: [ButtonModule, MenuModule],
  templateUrl: './sheet-add-menu.html',
  styleUrl: './sheet-add-menu.scss',
})
export class SheetAddMenu {
  public readonly tableId = input.required<number>();

  /** The section to add into, or null to add to the table itself. */
  public readonly sectionId = input<number | null>(null);

  /** Show just the plus, naming the action for assistive technology only. */
  public readonly iconOnly = input(false);

  public readonly sections = input<AddableSection[]>([]);

  /** The column blocks the table can take, for a horizontal table; only offered when adding to the table. */
  public readonly columnBlocks = input<AddableColumnBlock[]>([]);

  public readonly sectionChoices = computed<AddChoice[]>(() =>
    this.sections().map((addable) => ({
      label: addable.name,
      icon: 'pi pi-table',
      disabled: !addable.canAdd,
      add: () =>
        void this.store.addSection(this.tableId(), addable.templateSectionId, this.sectionId()),
    })),
  );

  public readonly columnChoices = computed<AddChoice[]>(() =>
    this.sectionId() === null
      ? this.columnBlocks().map((addable) => ({
          label: addable.name,
          icon: 'pi pi-arrows-h',
          disabled: !addable.canAdd,
          add: () => void this.store.addColumnBlock(this.tableId(), addable.templateColumnBlockId),
        }))
      : [],
  );

  public readonly hasChoices = computed(
    () => this.sectionChoices().length + this.columnChoices().length > 0,
  );

  /** The only thing that can be added here, when there's no choice to make. */
  public readonly only = computed<AddChoice | null>(() => {
    const choices = [...this.sectionChoices(), ...this.columnChoices()];
    return choices.length === 1 ? choices[0] : null;
  });

  /** The choices as a menu, under a heading each when there are both sections and columns to add. */
  public readonly menuItems = computed<MenuItem[]>(() => {
    const sections = this.sectionChoices().map(toMenuItem);
    const columns = this.columnChoices().map(toMenuItem);
    return sections.length > 0 && columns.length > 0
      ? [
          { label: 'Row group', items: sections },
          { label: 'Column', items: columns },
        ]
      : [...sections, ...columns];
  });

  public readonly label = computed(() => {
    const only = this.only();
    return only === null ? 'Add' : `Add ${only.label.toLowerCase()}`;
  });

  public readonly isBusy = computed(() => this.store.isBusy());

  private readonly menu = viewChild(Menu);
  private readonly store = inject(SheetStore);

  public add(event: Event): void {
    event.stopPropagation();
    const only = this.only();
    if (only !== null) {
      only.add();
    } else {
      this.menu()?.toggle(event);
    }
  }
}

function toMenuItem(choice: AddChoice): MenuItem {
  return {
    label: choice.label,
    icon: choice.icon,
    disabled: choice.disabled,
    command: choice.add,
  };
}
