import { Component, computed, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { PendingChange } from '../models/pending-change.model';

const MAX_SHOWN = 20;

/**
 * The Publish popover's list of what is about to go out: one link per changed table, section or row,
 * which goes to it on the sheet so it can be checked before publishing.
 */
@Component({
  selector: 'app-sheet-publish-changes',
  imports: [ButtonModule],
  templateUrl: './sheet-publish-changes.html',
  styleUrl: './sheet-publish-changes.scss',
})

export class SheetPublishChanges {
  public readonly changes = input.required<PendingChange[]>();

  /** The anchor key of the change the user picked. */
  public readonly chosen = output<string>();

  public readonly shown = computed(() => this.changes().slice(0, MAX_SHOWN));

  public readonly hiddenCount = computed(() => Math.max(0, this.changes().length - MAX_SHOWN));
}
