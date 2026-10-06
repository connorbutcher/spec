import { httpResource } from '@angular/common/http';
import { computed, inject, Injectable, linkedSignal, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';
import { reuseUnchanged } from '../../shared/reuse-unchanged.util';
import { apiErrorMessage } from '../templates/api-error-message';
import { CellType } from '../templates/models/cell-type.model';
import { CellValueRequest } from './models/cell-value-request.model';
import { SheetChange } from './models/sheet-change.model';
import { SheetIndex } from './models/sheet-index.model';
import { SheetSelection } from './models/sheet-selection.model';
import { SheetTarget } from './models/sheet-target.model';
import { SheetVersionSummary } from './models/sheet-version-summary.model';
import { SheetView } from './models/sheet-view.model';
import { Sheet } from './models/sheet.model';
import { addedId, buildSheetIndex, cellsById, sectionSiblings } from './sheet-index.util';
import { sheetUrl } from './sheet-url.util';
import { SheetsApi } from './sheets-api';

/**
 * State for the sheet screen: the sheet being looked at (a resource, live or at a version or date), the
 * cell types its cells use, what is selected, and every change the user makes. Provided by the sheet page.
 *
 * Changes save straight away and each returns the refreshed live sheet, which replaces the open copy.
 * Changes are sent one at a time, in the order they were made. A failed change shows its reason and
 * reloads the sheet, so the screen goes back to what the server has.
 *
 * A refreshed sheet keeps the objects of everything that did not change (see `reuseUnchanged`), so a
 * change only re-renders the tables, rows and cells it touched.
 */
@Injectable()
export class SheetStore {
  /** Which moment of the sheet is shown. Goes back to live whenever a different sheet is opened. */
  public readonly view = linkedSignal<SheetTarget | null, SheetView>({
    source: () => this.target(),
    computation: () => ({}),
  });

  /** Mark everything that changed after this version; null leaves the tables clean. */
  public readonly changesSince = linkedSignal<SheetTarget | null, number | null>({
    source: () => this.target(),
    computation: () => null,
  });

  public readonly sheet = computed<Sheet | null>(() =>
    this.sheetResource.hasValue() ? this.sheetResource.value() : null,
  );

  public readonly isLoading = computed(() => this.sheetResource.isLoading());
  public readonly hasError = computed(() => this.sheetResource.status() === 'error');

  /** Only the live view can be changed; a version or date is read-only. */
  public readonly canEdit = computed(() => this.sheet()?.isLive === true);

  public readonly index = computed<SheetIndex>(() => buildSheetIndex(this.sheet()));

  /** The sheet's published versions, newest first. */
  public readonly versions = computed<SheetVersionSummary[]>(() =>
    [...(this.sheet()?.versions ?? [])].sort((a, b) => b.versionNumber - a.versionNumber),
  );

  public readonly cellTypes = computed<ReadonlyMap<number, CellType>>(
    () =>
      new Map(
        (this.cellTypesResource.hasValue() ? this.cellTypesResource.value() : []).map((type) => [
          type.id,
          type,
        ]),
      ),
  );

  /** The selection, if everything it points at is still on the sheet. */
  public readonly selection = computed<SheetSelection | null>(() => {
    const selection = this.rawSelection();
    const index = this.index();
    if (selection === null || !index.tables.has(selection.tableId)) {
      return null;
    }
    if (selection.sectionId !== null && !index.sections.has(selection.sectionId)) {
      return null;
    }
    if (selection.rowId !== null && !index.rows.has(selection.rowId)) {
      return { ...selection, rowId: null };
    }
    return selection;
  });

  /** The reason the last change failed, until it's dismissed or another change is made. */
  public readonly error = signal<string | null>(null);

  public readonly isBusy = computed(() => this.inFlight() > 0);

  private readonly api = inject(SheetsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly rawSelection = signal<SheetSelection | null>(null);
  private readonly inFlight = signal(0);
  private tail: Promise<unknown> = Promise.resolve();

  private readonly target = toSignal(
    this.route.paramMap.pipe(
      map((params) => ({
        phaseId: Number(params.get('phaseId')),
        sheetTypeId: Number(params.get('sheetTypeId')),
      })),
    ),
    { initialValue: null },
  );

  private readonly sheetResource = httpResource<Sheet>(() => {
    const target = this.target();
    return target === null ? undefined : sheetUrl(target, this.view());
  });

  private readonly cellTypesResource = httpResource<CellType[]>(() => '/api/cell-types');

  public reload(): void {
    this.sheetResource.reload();
  }

  public setView(view: SheetView): void {
    this.view.set(view);
    this.rawSelection.set(null);
  }

  public setChangesSince(version: number | null): void {
    this.changesSince.set(version);
  }

  /** The change, if it happened after the version being compared against and so is to be marked. */
  public markedChange(change: SheetChange | null | undefined): SheetChange | null {
    const base = this.changesSince();
    return base !== null && change && change.versionNumber > base ? change : null;
  }

  public dismissError(): void {
    this.error.set(null);
  }

  public select(selection: SheetSelection | null): void {
    this.rawSelection.set(selection);
  }

  public selectSection(sectionId: number): void {
    const tableId = this.index().sectionTable.get(sectionId);
    if (tableId !== undefined) {
      this.rawSelection.set({ tableId, sectionId, rowId: null });
    }
  }

  public selectRow(rowId: number): void {
    const sectionId = this.index().rowSection.get(rowId);
    const tableId = sectionId === undefined ? undefined : this.index().sectionTable.get(sectionId);
    if (sectionId !== undefined && tableId !== undefined) {
      this.rawSelection.set({ tableId, sectionId, rowId });
    }
  }

  public async addTable(tableTemplateId: number): Promise<void> {
    const sheetId = this.sheet()?.id;
    if (sheetId === undefined) {
      return;
    }
    const before = this.index().tables;
    const sheet = await this.run(() => this.api.addTable(sheetId, tableTemplateId));
    const added = sheet ? addedId(before, this.index().tables) : undefined;
    if (added !== undefined) {
      this.rawSelection.set({ tableId: added, sectionId: null, rowId: null });
    }
  }

  public async setTableTitle(tableId: number, title: string | null): Promise<void> {
    await this.run(() => this.api.setTableTitle(tableId, title));
  }

  /** Put the table at a 0-based place in the sheet's table order. */
  public async moveTableTo(tableId: number, index: number): Promise<void> {
    await this.run(() => this.api.moveTable(tableId, index + 1));
  }

  public async removeTable(tableId: number): Promise<void> {
    await this.run(() => this.api.removeTable(tableId));
  }

  public async addSection(
    tableId: number,
    templateSectionId: number,
    parentSheetSectionId: number | null,
  ): Promise<void> {
    const before = this.index().sections;
    const sheet = await this.run(() =>
      this.api.addSection(tableId, templateSectionId, parentSheetSectionId),
    );
    const added = sheet ? addedId(before, this.index().sections) : undefined;
    if (added !== undefined) {
      this.rawSelection.set({ tableId, sectionId: added, rowId: null });
    }
  }

  public async moveSection(sectionId: number, step: -1 | 1): Promise<void> {
    const siblings = sectionSiblings(this.index(), sectionId);
    const position = siblings.findIndex((section) => section.id === sectionId) + 1 + step;
    await this.run(() => this.api.moveSection(sectionId, position));
  }

  public async removeSection(sectionId: number): Promise<void> {
    const index = this.index();
    const tableId = index.sectionTable.get(sectionId);
    const parentId = index.sectionParent.get(sectionId) ?? null;
    const sheet = await this.run(() => this.api.removeSection(sectionId));
    if (sheet && tableId !== undefined) {
      this.rawSelection.set({ tableId, sectionId: parentId, rowId: null });
    }
  }

  public async addColumnBlock(tableId: number, templateColumnBlockId: number): Promise<void> {
    await this.run(() => this.api.addColumnBlock(tableId, templateColumnBlockId));
  }

  public async moveColumnBlock(
    tableId: number,
    columnBlockId: number,
    step: -1 | 1,
  ): Promise<void> {
    const blocks = this.index().tables.get(tableId)?.columnBlocks ?? [];
    const position = blocks.findIndex((block) => block.id === columnBlockId) + 1 + step;
    await this.run(() => this.api.moveColumnBlock(columnBlockId, position));
  }

  public async removeColumnBlock(columnBlockId: number): Promise<void> {
    await this.run(() => this.api.removeColumnBlock(columnBlockId));
  }

  public async saveValues(rowId: number, values: CellValueRequest[]): Promise<void> {
    await this.run(
      () => this.api.saveValues(rowId, values),
      values.map((value) => value.sheetCellId),
    );
  }

  public async moveRow(rowId: number, step: -1 | 1): Promise<void> {
    const index = this.index();
    const section = index.sections.get(index.rowSection.get(rowId) ?? -1);
    const position = (section?.rows.findIndex((row) => row.id === rowId) ?? 0) + 1 + step;
    await this.run(() => this.api.moveRow(rowId, position));
  }

  public async removeRow(rowId: number): Promise<void> {
    const sectionId = this.index().rowSection.get(rowId);
    const tableId = this.index().sectionTable.get(sectionId ?? -1);
    const sheet = await this.run(() => this.api.removeRow(rowId));
    if (sheet && sectionId !== undefined && tableId !== undefined) {
      this.rawSelection.set({ tableId, sectionId, rowId: null });
    }
  }

  public async publish(note: string | null): Promise<boolean> {
    const sheetId = this.sheet()?.id;
    if (sheetId === undefined) {
      return false;
    }
    return (await this.run(() => this.api.publish(sheetId, note))) !== null;
  }

  /**
   * Queues a change. The cells in `savedCellIds` always come back as new objects, so their editors
   * drop what was typed and show what the server stored, even when that is what was there before.
   */
  private run(change: () => Promise<Sheet>, savedCellIds: number[] = []): Promise<Sheet | null> {
    const task = this.tail.then(() => this.execute(change, savedCellIds));
    this.tail = task;
    return task;
  }

  private async execute(
    change: () => Promise<Sheet>,
    savedCellIds: number[],
  ): Promise<Sheet | null> {
    this.inFlight.update((count) => count + 1);
    this.error.set(null);
    try {
      const refreshed = await change();
      const saved = cellsById(refreshed, savedCellIds);
      const sheet = reuseUnchanged(this.sheet(), refreshed, (part) => saved.has(part));
      this.sheetResource.set(sheet);
      return sheet;
    } catch (failure) {
      this.error.set(apiErrorMessage(failure));
      this.sheetResource.reload();
      return null;
    } finally {
      this.inFlight.update((count) => count - 1);
    }
  }
}
