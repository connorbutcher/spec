import { Component, input } from '@angular/core';
import { CanvasSection } from '../canvas-section/canvas-section';
import { TemplateLayout } from '../models/template-layout.model';

/** The table preview: one CSS grid whose top-level sections are subgrids. Click anything to configure it. */
@Component({
  selector: 'app-template-canvas',
  imports: [CanvasSection],
  templateUrl: './template-canvas.html',
  styleUrl: './template-canvas.scss',
})
export class TemplateCanvas {
  public readonly layout = input.required<TemplateLayout>();
}
