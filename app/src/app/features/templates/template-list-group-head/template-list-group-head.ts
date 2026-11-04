import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TemplateListGroup } from '../models/template-list-group.model';

/** The header of one sheet type in the template list: its icon, its name and a button that adds a table to it. */
@Component({
  selector: 'app-template-list-group-head',
  imports: [ButtonModule],
  templateUrl: './template-list-group-head.html',
  styleUrl: './template-list-group-head.scss',
})
export class TemplateListGroupHead {
  public readonly group = input.required<TemplateListGroup>();
  public readonly disabled = input(false);

  public readonly added = output();

  public add(event: Event): void {
    // The button sits in the list's group header; keep the click from reaching the list.
    event.stopPropagation();
    this.added.emit();
  }
}
