import { Component, computed, inject, input } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ColumnBlockLayout } from '../../templates/models/column-block-layout.model';
import { GridStyle } from '../../templates/models/grid-style';
import { SheetColumnBlock } from '../models/sheet-column-block.model';
import { SheetTable } from '../models/sheet-table.model';
import { changeLabel } from '../sheet-labels.util';
import { SheetStore } from '../sheet.store';

/**
 * One copy of a column block on a horizontal table. It draws nothing over the cells; it only holds the
 * copy's own controls in the top corner of its columns: move left, move right and remove (one delete for
 * the whole copy, with its cells in every row), plus the "changed in" tag when markers are on.
 */
@Component({
  selector: 'app-sheet-grid-column-block',
  imports: [ButtonModule, TagModule, TooltipModule],
  templateUrl: './sheet-grid-column-block.html',
  styleUrl: './sheet-grid-column-block.scss',
  host: {
    '[style]': 'hostStyle()',
  },
})
export class SheetGridColumnBlock {
  public readonly layout = input.required<ColumnBlockLayout>();

  public readonly table = input.required<SheetTable>();

  public readonly block = computed<SheetColumnBlock | null>(
    () => this.table().columnBlocks.find((block) => block.id === this.layout().block.id) ?? null,
  );

  public readonly canEdit = computed(() => {
    const block = this.block();
    const lock = block?.lock ?? null;
    return this.store.canEdit() && block !== null && (lock === null || lock.isMine);
  });

  public readonly position = computed(() =>
    this.table().columnBlocks.findIndex((block) => block.id === this.layout().block.id),
  );

  public readonly canMoveLeft = computed(() => this.position() > 0);

  public readonly canMoveRight = computed(() => {
    const index = this.position();
    return index >= 0 && index < this.table().columnBlocks.length - 1;
  });

  public readonly canRemove = computed(() => this.block()?.canRemove === true);

  public readonly changeLabel = computed(() => {
    const change = this.block()?.lastChange ?? null;
    return this.store.isMarked(change) && change !== null ? changeLabel(change) : null;
  });

  public readonly isBusy = computed(() => this.store.isBusy());

  private readonly store = inject(SheetStore);
  private readonly confirmation = inject(ConfirmationService);

  public hostStyle(): GridStyle {
    return this.layout().style;
  }

  public move(step: -1 | 1): void {
    void this.store.moveColumnBlock(this.table().id, this.layout().block.id, step);
  }

  public askRemove(event: Event): void {
    const block = this.block();
    if (block === null) {
      return;
    }
    this.confirmation.confirm({
      target: event.currentTarget as EventTarget,
      message: `Remove this ${block.name}? Its cells in every row go too.`,
      acceptLabel: 'Remove',
      rejectLabel: 'Cancel',
      acceptButtonProps: { size: 'small', severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', size: 'small', outlined: true },
      accept: () => void this.store.removeColumnBlock(block.id),
    });
  }
}
