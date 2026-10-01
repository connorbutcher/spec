import { Component, inject, input } from '@angular/core';
import { CanvasCell } from '../canvas-cell/canvas-cell';
import { CanvasMenu } from '../canvas-menu';
import { GridStyle } from '../models/grid-style';
import { SectionLayout } from '../models/section-layout.model';
import { PanelNavigator } from '../panel-navigator';

/**
 * A section in the preview. It draws nothing of its own: the host element is an invisible subgrid that
 * takes the section's place in its parent, so its rows and cells line up with the table. Hovering shows
 * its extent as a thin outline; selecting it (from the outline, a cell's menu, the panel or the
 * breadcrumb) outlines it in the accent colour. Right-click opens the quick-edit menu.
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
  private readonly menu = inject(CanvasMenu);

  public hostStyle(): GridStyle {
    return this.layout().style;
  }

  public isSelected(): boolean {
    return this.navigator.isShowing({ kind: 'section', id: this.layout().section.id });
  }

  public openMenu(event: MouseEvent): void {
    this.menu.openForSection(event, this.layout().section.id);
  }
}
