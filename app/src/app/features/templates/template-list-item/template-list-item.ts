import { Component, input } from '@angular/core';
import { TableTemplateSummary } from '../models/table-template-summary.model';

/** One table in the template list: an orientation icon, its name and its latest version number. */
@Component({
  selector: 'app-template-list-item',
  templateUrl: './template-list-item.html',
  styleUrl: './template-list-item.scss',
})
export class TemplateListItem {
  public readonly template = input.required<TableTemplateSummary>();
}
