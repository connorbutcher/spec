import { Component, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { CanvasCell } from '../canvas-cell/canvas-cell';
import { CanvasMenu } from '../canvas-menu';
import { GridStyle } from '../models/grid-style';
import { SectionLayout } from '../models/section-layout.model';
import { PanelNavigator } from '../panel-navigator';
import { describeInstances, isHeader, sectionNoun } from '../section-role.util';
import { TemplatesStore } from '../templates.store';

/**
 * A section in the preview. It draws nothing of its own: the host element is an invisible subgrid that
 * takes the section's place in its parent, so its rows and cells line up with the table. Hovering shows
 * a faint outline with the section's name; selecting it (from a cell's menu, the panel, the structure
 * tree or the breadcrumb) outlines it.
 *
 * An addable section ends with a dashed area that says more sub-sections can be added, and adds one.
 */
@Component({
  selector: 'app-canvas-section',
  imports: [ButtonModule, CanvasCell, TooltipModule],
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
  private readonly store = inject(TemplatesStore);

  public hostStyle(): GridStyle {
    return this.layout().style;
  }

  public isSelected(): boolean {
    return this.navigator.isShowing({ kind: 'section', id: this.layout().section.id });
  }

  /** The hover label: the name and how it's added, e.g. "Readings · Added on the sheet: any number". */
  public label(): string {
    const section = this.layout().section;
    return `${section.name} · ${isHeader(section) ? 'Header' : describeInstances(section)}`;
  }

  /** Only addable sections hold sub-sections; the header holds rows only. */
  public canAddSubSection(): boolean {
    return !isHeader(this.layout().section);
  }

  public hasNoRows(): boolean {
    return this.layout().section.rows.length === 0;
  }

  public isBusy(): boolean {
    return this.store.isSaving() || !this.store.canEdit();
  }

  public tooltip(): string {
    return `People add sub-sections inside "${this.layout().section.name}" on the sheet, as often as needed.`;
  }

  public noun(): string {
    return sectionNoun(this.layout().section).toLowerCase();
  }

  public openMenu(event: MouseEvent): void {
    this.menu.openForSection(event, this.layout().section.id);
  }

  public async addRow(): Promise<void> {
    const id = await this.store.addRow(this.layout().section.id);
    if (id !== null) {
      this.navigator.open({ kind: 'row', id });
    }
  }

  public async addSubSection(): Promise<void> {
    const id = await this.store.addSection(this.layout().section.id);
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }
}
