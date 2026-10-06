import { Component, computed, input } from '@angular/core';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { SheetChange } from '../models/sheet-change.model';
import { changeLabel, versionLabel } from '../sheet-labels.util';

/** The "changed in" tag on a section or column block: the version, with when and by whom in a tooltip. */
@Component({
  selector: 'app-sheet-change-tag',
  imports: [TagModule, TooltipModule],
  templateUrl: './sheet-change-tag.html',
  styleUrl: './sheet-change-tag.scss',
})
export class SheetChangeTag {
  public readonly change = input.required<SheetChange>();

  public readonly tooltipPosition = input<'right' | 'bottom'>('right');

  public readonly version = computed(() => versionLabel(this.change().versionNumber));

  public readonly detail = computed(() => changeLabel(this.change()));
}
