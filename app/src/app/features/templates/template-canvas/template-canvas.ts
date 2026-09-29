import { afterNextRender, Component, inject, input, viewChild } from '@angular/core';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { CanvasMenu } from '../canvas-menu';
import { CanvasSection } from '../canvas-section/canvas-section';
import { TemplateLayout } from '../models/template-layout.model';

/**
 * The table preview: one CSS grid whose top-level sections are invisible subgrids, so only rows and
 * cells show. Click a cell to configure it; right-click (or use the menu key) for the quick-edit menu.
 */
@Component({
  selector: 'app-template-canvas',
  imports: [CanvasSection, ContextMenuModule],
  providers: [CanvasMenu],
  templateUrl: './template-canvas.html',
  styleUrl: './template-canvas.scss',
})
export class TemplateCanvas {
  public readonly layout = input.required<TemplateLayout>();

  public readonly menu = inject(CanvasMenu);

  private readonly contextMenu = viewChild.required(ContextMenu);

  constructor() {
    afterNextRender(() => this.menu.register(this.contextMenu()));
  }

  public openMenu(event: MouseEvent): void {
    this.menu.openForCanvas(event);
  }
}
