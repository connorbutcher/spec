import { Component } from '@angular/core';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';

/** Shown in the middle until a table is picked from the list. */
@Component({
  selector: 'app-template-select-prompt',
  imports: [EmptyState],
  templateUrl: './template-select-prompt.html',
  styleUrl: './template-select-prompt.scss',
})
export class TemplateSelectPrompt {}
