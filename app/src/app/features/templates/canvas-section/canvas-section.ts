import { Component, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { CanvasCell } from '../canvas-cell/canvas-cell';
import { GridStyle } from '../models/grid-style';
import { SectionLayout } from '../models/section-layout.model';
import { PanelNavigator } from '../panel-navigator';
import { describeInstances } from '../section-role.util';
import { TemplatesStore } from '../templates.store';

/**
 * A section in the preview. The host element is the grid item: it takes the section's place in its
 * parent and is itself a subgrid, so its header, child sections and cells all line up with the table.
 * The header shows the section's role: a lock for an always-included block, a tag for a repeating
 * or optional one.
 */
@Component({
  selector: 'app-canvas-section',
  imports: [ButtonModule, CanvasCell, TagModule, TooltipModule],
  templateUrl: './canvas-section.html',
  styleUrl: './canvas-section.scss',
  host: {
    '[style]': 'hostStyle()',
    '[class.selected]': 'isSelected()',
    '[class.repeating]': "layout().section.role === 'Repeating'",
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

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  public roleText(): string {
    return describeInstances(this.layout().section);
  }

  /** "×1+", "×1–5": how many copies a sheet table can hold. */
  public repeatText(): string {
    const { minInstances, maxInstances } = this.layout().section;
    return maxInstances === null ? `×${minInstances}+` : `×${minInstances}–${maxInstances}`;
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
