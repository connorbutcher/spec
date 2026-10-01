import { Component, computed, inject, input, viewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { Menu, MenuModule } from 'primeng/menu';
import { AddableSection } from '../models/addable-section.model';
import { SheetStore } from '../sheet.store';

/**
 * The one add button for a place that can grow: a table or a group section, listing the section types it takes. When a single section can be
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

  public readonly choices = computed<MenuItem[]>(() => {
    const sectionId = this.sectionId();
    const sections: MenuItem[] = this.sections().map((addable) => ({
      label: addable.name,
      icon: 'pi pi-table',
      disabled: !addable.canAdd,
      command: () =>
        void this.store.addSection(this.tableId(), addable.templateSectionId, sectionId),
    }));
    return sections;
  });

  /** The only thing that can be added here, when there's no choice to make. */
  public readonly only = computed<MenuItem | null>(() => {
    const choices = this.choices();
    return choices.length === 1 && choices[0].items === undefined ? choices[0] : null;
  });

  public readonly label = computed(() => {
    const only = this.only();
    if (only === null) {
      return 'Add';
    }
    return `Add ${only.label?.toLowerCase()}`;
  });

  public readonly isBusy = computed(() => this.store.isBusy());

  private readonly menu = viewChild(Menu);
  private readonly store = inject(SheetStore);

  public add(event: Event): void {
    event.stopPropagation();
    const only = this.only();
    if (only !== null) {
      only.command?.({ originalEvent: event, item: only });
    } else {
      this.menu()?.toggle(event);
    }
  }
}
