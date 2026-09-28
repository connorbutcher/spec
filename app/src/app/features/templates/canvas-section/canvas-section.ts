import { Component, inject, input } from '@angular/core';
import { CanvasCell } from '../canvas-cell/canvas-cell';
import { GridStyle } from '../models/grid-style';
import { SectionLayout } from '../models/section-layout.model';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

/**
 * A section in the preview. The host element is the grid item: it takes the section's place in its
 * parent and is itself a subgrid, so its header, child sections and cells all line up with the table.
 */
@Component({
  selector: 'app-canvas-section',
  imports: [CanvasCell],
  templateUrl: './canvas-section.html',
  styleUrl: './canvas-section.scss',
  host: {
    '[style]': 'hostStyle()',
    '[class.selected]': 'isSelected()',
  },
})
export class CanvasSection {
  public readonly layout = input.required<SectionLayout>();

  private readonly navigator = inject(PanelNavigator);
  private readonly store = inject(TemplatesStore);

  public hostStyle(): GridStyle {
    return this.layout().style;
  }

  public isSelected(): boolean {
    return this.navigator.isShowing({ kind: 'section', id: this.layout().section.id });
  }

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public open(): void {
    this.navigator.open({ kind: 'section', id: this.layout().section.id });
  }

  public async addRow(): Promise<void> {
    const id = await this.store.addRow(this.layout().section.id);
    if (id !== null) {
      this.navigator.open({ kind: 'row', id });
    }
  }
}
