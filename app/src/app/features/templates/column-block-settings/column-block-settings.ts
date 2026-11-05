import { Component, computed, inject, input } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { InstanceCountsFields } from '../instance-counts-fields/instance-counts-fields';
import { ColumnBlockEntry } from '../models/column-block-entry.model';
import { InstanceCounts } from '../models/instance-counts.model';
import { UpdateTemplateColumnBlockRequest } from '../models/update-template-column-block-request.model';
import { SectionLinksUtil } from '../section-links.util';
import { SettingsGroup } from '../settings-group/settings-group';
import { StickyColumnsField } from '../sticky-columns-field/sticky-columns-field';
import { TemplateLayoutUtil } from '../template-layout.util';
import { TemplatesStore } from '../templates.store';

/**
 * Panel page for a column block of a horizontal table: its name, its pinned columns and how many
 * copies a sheet table has. Moving and deleting are in the selection bar. Every row holds the block's cells for that row, so each copy runs
 * through the whole table; its cells are edited in the rows.
 */
@Component({
  selector: 'app-column-block-settings',
  imports: [InputTextModule, InstanceCountsFields, SettingsGroup, StickyColumnsField],
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
      : `${SectionLinksUtil.plural(cells, 'cell')} in ${SectionLinksUtil.plural(counts.length, 'row')}.`;
  });

  /** How many columns the block takes: the most of it that can be pinned. */
  public readonly blockColumns = computed(() => {
    const template = this.store.template();
    return template
      ? (TemplateLayoutUtil.planColumns(template).blockWidths.get(this.columnBlockId()) ?? 1)
      : 1;
  });

  private readonly store = inject(TemplatesStore);

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

  public setCounts(counts: InstanceCounts): void {
    void this.save(counts);
  }

  public setSticky(count: number): void {
    void this.save({ stickyColumnCount: count });
  }

  private save(changes: Partial<UpdateTemplateColumnBlockRequest>): Promise<void> {
    return this.store.updateColumnBlock(this.columnBlockId(), changes);
  }
}
