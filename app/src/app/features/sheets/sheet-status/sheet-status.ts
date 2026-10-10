import { Component, computed, input } from '@angular/core';
import { TagModule } from 'primeng/tag';
import { stateIcon, stateText, statusTone, versionText } from '../item-status.util';
import { ItemStatus } from '../models/item-status.model';

/**
 * Where an item stands, written the one way it is written everywhere: the version it is on ("v7", or a
 * New tag), then its state and who has it ("Draft · A. Smith") with the state's icon, in the colour of
 * whose it is. A published item shows its version and nothing else.
 *
 * Used for a row in the status column, for the selected row in the action bar, and for a table in its
 * header, so the same words, icon and colour mean the same thing in each place.
 */
@Component({
  selector: 'app-sheet-status',
  imports: [TagModule],
  templateUrl: './sheet-status.html',
  styleUrl: './sheet-status.scss',
})
export class SheetStatus {
  public readonly status = input.required<ItemStatus>();

  /** The item changed after the version being compared against: its version is written in bold. */
  public readonly changed = input(false);

  public readonly version = computed(() => versionText(this.status()));

  public readonly text = computed(() => stateText(this.status()));

  public readonly icon = computed(() => stateIcon(this.status().state));

  public readonly tone = computed(() => statusTone(this.status()));
}
