import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RadioButtonModule } from 'primeng/radiobutton';
import { PublishScopeOption } from '../models/publish-scope-option.model';
import { PublishScope } from '../models/publish-scope';

/**
 * The choice of what a publish takes: only the user's own changes, or everything waiting on the sheet.
 * Each choice shows how many changes it would send out, and one with nothing in it can't be chosen.
 */
@Component({
  selector: 'app-sheet-publish-scope',
  imports: [FormsModule, RadioButtonModule],
  templateUrl: './sheet-publish-scope.html',
  styleUrl: './sheet-publish-scope.scss',
})
export class SheetPublishScope {
  public readonly options = input.required<PublishScopeOption[]>();

  public readonly scope = input.required<PublishScope>();

  public readonly scopeChange = output<PublishScope>();
}
