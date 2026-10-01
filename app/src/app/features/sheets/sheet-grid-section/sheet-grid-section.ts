import { Component, computed, inject, input } from '@angular/core';
import { GridStyle } from '../../templates/models/grid-style';
import { SectionLayout } from '../../templates/models/section-layout.model';
import { SheetGridCell } from '../sheet-grid-cell/sheet-grid-cell';
import { SheetStore } from '../sheet.store';

/**
 * A section copy on the sheet grid. It draws nothing of its own: the host is an invisible subgrid that
 * takes the section's place in its parent, so its rows and cells line up with the table. Clicking a bare
 * part of it selects the section; the selected section gets a thin outline.
 */
@Component({
  selector: 'app-sheet-grid-section',
  imports: [SheetGridCell],
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

  private readonly store = inject(SheetStore);

  public hostStyle(): GridStyle {
    return this.layout().style;
  }

  public select(event: Event): void {
    event.stopPropagation();
    this.store.selectSection(this.layout().section.id);
  }
}
