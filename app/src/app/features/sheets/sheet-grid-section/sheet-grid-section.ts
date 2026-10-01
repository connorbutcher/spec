import { Component, computed, inject, input } from '@angular/core';
import { GridStyle } from '../../templates/models/grid-style';
import { SectionLayout } from '../../templates/models/section-layout.model';
import { SheetAddMenu } from '../sheet-add-menu/sheet-add-menu';
import { SheetGridCell } from '../sheet-grid-cell/sheet-grid-cell';
import { sectionAncestors } from '../sheet-index.util';
import { SheetStore } from '../sheet.store';

/**
 * A section copy on the sheet grid. It draws nothing of its own: the host is an invisible subgrid that
 * takes the section's place in its parent, so its rows and cells line up with the table. Clicking a bare
 * part of it selects the section; the selected section gets a thin outline.
 */
@Component({
  selector: 'app-sheet-grid-section',
  imports: [SheetAddMenu, SheetGridCell],
  templateUrl: './sheet-grid-section.html',
  styleUrl: './sheet-grid-section.scss',
  host: {
    '[style]': 'hostStyle()',
    '[class.selected]': 'isSelected()',
    '(click)': 'select($event)',
  },
})
export class SheetGridSection {
  public readonly layout = input.required<SectionLayout>();

  public readonly isSelected = computed(() => {
    const selection = this.store.selection();
    return selection?.sectionId === this.layout().section.id && selection.rowId === null;
  });

  /** The sheet's copy of this section, which knows what can be added to it. */
  public readonly sheetSection = computed(
    () => this.store.index().sections.get(this.layout().section.id) ?? null,
  );

  public readonly tableId = computed(
    () => this.store.index().sectionTable.get(this.layout().section.id) ?? -1,
  );

  /** Whether the group offers an add control: it can take sections and the sheet is live. */
  public readonly canAdd = computed(() => {
    const section = this.sheetSection();
    return this.store.canEdit() && section !== null && section.addableSections.length > 0;
  });

  /** Nested groups put their button further along the gutter so two never sit on top of each other. */
  public readonly gutterOffset = computed(() => {
    const depth = sectionAncestors(this.store.index(), this.layout().section.id).length;
    return `calc(100% + 6px + ${depth * 90}px)`;
  });

  private readonly store = inject(SheetStore);

  public hostStyle(): GridStyle {
    return this.layout().style;
  }

  public select(event: Event): void {
    event.stopPropagation();
    this.store.selectSection(this.layout().section.id);
  }
}
