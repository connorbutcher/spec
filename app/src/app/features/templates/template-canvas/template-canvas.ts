import { Component, inject, input } from '@angular/core';
import { CanvasColumnBlock } from '../canvas-column-block/canvas-column-block';
import { CanvasSection } from '../canvas-section/canvas-section';
import { TemplateLayout } from '../models/template-layout.model';
import { StructureMenu } from '../structure-menu';

/**
 * The table preview: one CSS grid whose top-level sections are invisible subgrids, so only rows and
 * cells show, with a dashed frame over each column block of a horizontal table. Click a cell to
 * configure it; right-click (or use the menu key) for its menu, or on empty space for the table's.
 */
@Component({
  selector: 'app-template-canvas',
  imports: [CanvasColumnBlock, CanvasSection],
  templateUrl: './template-canvas.html',
  styleUrl: './template-canvas.scss',
})
export class TemplateCanvas {
  public readonly layout = input.required<TemplateLayout>();

  private readonly menu = inject(StructureMenu);

  public openMenu(event: MouseEvent): void {
    this.menu.open(event, { kind: 'template' });
  }
}
