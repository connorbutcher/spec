import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MultiSelectModule } from 'primeng/multiselect';
import { Approver } from '../../approvals/models/approver.model';

/**
 * Who has to approve a publish: at least one person, and every one chosen must approve before anything
 * is published. The list leaves out the user, who can't approve their own changes.
 */
@Component({
  selector: 'app-sheet-publish-approvers',
  imports: [FormsModule, MultiSelectModule],
  templateUrl: './sheet-publish-approvers.html',
  styleUrl: './sheet-publish-approvers.scss',
})

export class SheetPublishApprovers {
  public readonly approvers = input.required<Approver[]>();

  public readonly selected = input.required<number[]>();

  public readonly isLoading = input(false);

  public readonly selectedChange = output<number[]>();
}
