import { Component, computed, inject, input } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { withCount } from '../instance-counts.util';
import { ColumnBlockEntry } from '../models/column-block-entry.model';
import { InstanceField } from '../models/instance-field';
import { UpdateTemplateColumnBlockRequest } from '../models/update-template-column-block-request.model';
import { MoveButtons } from '../move-buttons/move-buttons';
import { NumberField } from '../number-field/number-field';
import { PanelNavigator } from '../panel-navigator';
import { plural } from '../section-links.util';
import { TemplatesStore } from '../templates.store';

/**
 * Panel page for a column block of a horizontal table: its name, its place among the blocks and how
 * many copies a sheet table has. Every row holds the block's cells for that row, so each copy runs
 * through the whole table; its cells are edited in the rows.
 */
@Component({
  selector: 'app-column-block-settings',
  imports: [ConfirmDeleteButton, InputTextModule, MoveButtons, NumberField],
  templateUrl: './column-block-settings.html',
  styleUrl: './column-block-settings.scss',
})
export class ColumnBlockSettings {
  public readonly columnBlockId = input.required<number>();

  public readonly entry = computed<ColumnBlockEntry | undefined>(() =>
    this.store.index().columnBlocks.get(this.columnBlockId()),
  );

  /** "4 cells in 3 rows": how much of the table the block covers. */
  public readonly cellSummary = computed(() => {
    const id = this.columnBlockId();
    const counts = [...this.store.index().rows.values()]
      .map((entry) => entry.row.cells.filter((cell) => cell.columnBlockId === id).length)
      .filter((count) => count > 0);
    const cells = counts.reduce((total, count) => total + count, 0);
    return cells === 0
      ? 'No cells yet.'
      : `${plural(cells, 'cell')} in ${plural(counts.length, 'row')}.`;
  });

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public blockCount(): number {
    return this.store.index().columnBlocks.size;
  }

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  public async rename(input: HTMLInputElement): Promise<void> {
    const block = this.entry()?.block;
    const name = input.value.trim();
    if (block && name && name !== block.name) {
      await this.save({ name });
    }
    input.value = this.entry()?.block.name ?? '';
  }

  /** Saves one of the counts, nudging the others so fewest <= starts with <= most still holds. */
  public setCount(field: InstanceField, value: number | null): void {
    const block = this.entry()?.block;
    if (block) {
      void this.save(withCount(block, field, value));
    }
  }

  public move(position: number): void {
    void this.store.moveColumnBlock(this.columnBlockId(), position);
  }

  public async delete(): Promise<void> {
    if (await this.store.deleteColumnBlock(this.columnBlockId())) {
      this.navigator.forgetMissing({ kind: 'template' });
    }
  }

  private save(changes: Partial<UpdateTemplateColumnBlockRequest>): Promise<void> {
    return this.store.updateColumnBlock(this.columnBlockId(), changes);
  }
}
