import { Component, computed, inject, input } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ColumnBlockLayout } from '../../templates/models/column-block-layout.model';
import { SheetColumnBlock } from '../models/sheet-column-block.model';
import { SheetTable } from '../models/sheet-table.model';
import { SheetChange } from '../models/sheet-change.model';
import { SheetChangeTag } from '../sheet-change-tag/sheet-change-tag';
import { confirmRemoval } from '../sheet-confirm.util';
import { otherUsersLock } from '../sheet-lock.util';
import { SheetStore } from '../sheet.store';

/**
 * One copy of a column block on a horizontal table. It draws nothing over the cells; it only holds the
 * copy's own controls in the top corner of its columns: move left, move right and remove (one delete for
 * the whole copy, with its cells in every row), plus the "changed in" tag when markers are on.
 */
@Component({
  selector: 'app-sheet-grid-column-block',
  imports: [ButtonModule, SheetChangeTag],
  templateUrl: './sheet-grid-column-block.html',
  styleUrl: './sheet-grid-column-block.scss',
  host: {
    '[style]': 'layout().style',
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
    return this.store.canEdit() && block !== null && otherUsersLock(block.lock) === null;
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

  /** What last added or moved the block, if that was after the compared version. */
  public readonly change = computed<SheetChange | null>(() =>
    this.store.markedChange(this.block()?.lastChange),
  );

  public readonly controlsLabel = computed(
    () => `${this.block()?.name ?? 'Column'} column controls`,
  );

  public readonly isBusy = computed(() => this.store.isBusy());

  private readonly store = inject(SheetStore);
  private readonly confirmation = inject(ConfirmationService);

  public move(step: -1 | 1): void {
    void this.store.moveColumnBlock(this.table().id, this.layout().block.id, step);
  }

  public askRemove(event: Event): void {
    const block = this.block();
    if (block === null) {
      return;
    }
    confirmRemoval(
      this.confirmation,
      event,
      `Remove this ${block.name}? Its cells in every row go too.`,
      () => this.store.removeColumnBlock(block.id),
    );
  }
}
