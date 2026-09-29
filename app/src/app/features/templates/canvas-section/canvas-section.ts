import { Component, inject, input } from '@angular/core';
import { CanvasCell } from '../canvas-cell/canvas-cell';
import { CanvasMenu } from '../canvas-menu';
import { GridStyle } from '../models/grid-style';
import { SectionLayout } from '../models/section-layout.model';
import { PanelNavigator } from '../panel-navigator';
import { describeInstances } from '../section-role.util';

/**
 * A section in the preview. It draws nothing of its own: the host element is an invisible subgrid that
 * takes the section's place in its parent, so its rows and cells line up with the table. Hovering shows
 * a faint outline with the section's name; selecting it (from a cell's menu, the panel or the
 * breadcrumb) outlines it.
 */
@Component({
  selector: 'app-canvas-section',
  imports: [CanvasCell],
  templateUrl: './canvas-section.html',
  styleUrl: './canvas-section.scss',
  host: {
    '[style]': 'hostStyle()',
    '[class.selected]': 'isSelected()',
    '[attr.data-label]': 'label()',
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

  /** The hover label: the name and its role, e.g. "Readings · Repeats: 0 min, any number". */
  public label(): string {
    const section = this.layout().section;
    const isHeader = section.parentSectionId === null && section.role === 'Fixed';
    return `${section.name} · ${isHeader ? 'Header' : describeInstances(section)}`;
  }

  public open(): void {
    this.navigator.open({ kind: 'section', id: this.layout().section.id });
  }

  public openMenu(event: MouseEvent): void {
    this.menu.openForSection(event, this.layout().section.id);
  }
}
