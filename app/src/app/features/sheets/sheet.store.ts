import { httpResource } from '@angular/common/http';
import { computed, inject, Injectable, linkedSignal, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';
import { apiErrorMessage } from '../templates/api-error-message';
import { CellType } from '../templates/models/cell-type.model';
import { CellValueRequest } from './models/cell-value-request.model';
import { SheetIndex } from './models/sheet-index.model';
import { SheetSelection } from './models/sheet-selection.model';
import { SheetView } from './models/sheet-view.model';
import { Sheet } from './models/sheet.model';
import { buildSheetIndex, rowIds, sectionIds, sectionSiblings } from './sheet-index.util';
import { SheetsApi } from './sheets-api';

interface SheetTarget {
  phaseId: number;
  sheetTypeId: number;
}

/**
 * State for the sheet screen: the sheet being looked at (a resource, live or at a version or date), the
 * cell types its cells use, what is selected, and every change the user makes. Provided by the sheet page.
 *
 * Changes save straight away and each returns the refreshed live sheet, which replaces the open copy.
 * Changes are sent one at a time, in the order they were made. A failed change shows its reason and
 * reloads the sheet, so the screen goes back to what the server has.
 */
@Injectable()
export class SheetStore {
  /** Which moment of the sheet is shown. Goes back to live whenever a different sheet is opened. */
  public readonly view = linkedSignal<SheetTarget | null, SheetView>({
    source: () => this.target(),
    computation: () => ({}),
  });

  public readonly sheet = computed<Sheet | null>(() =>
    this.sheetResource.hasValue() ? this.sheetResource.value() : null,
  );

  public readonly isLoading = computed(() => this.sheetResource.isLoading());
  public readonly hasError = computed(() => this.sheetResource.status() === 'error');

  /** Only the live view can be changed; a version or date is read-only. */
  public readonly canEdit = computed(() => this.sheet()?.isLive === true);

  public readonly index = computed<SheetIndex>(() => buildSheetIndex(this.sheet()));

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
  private readonly lockRequests = new Set<number>();
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
    if (target === null) {
      return undefined;
    }
    const view = this.view();
    const query = view.version
      ? `?version=${view.version}`
      : view.asOf
        ? `?asOf=${encodeURIComponent(view.asOf)}`
        : '';
    return `/api/phases/${target.phaseId}/sheets/${target.sheetTypeId}${query}`;
  });

  private readonly cellTypesResource = httpResource<CellType[]>(() => '/api/cell-types');

  public reload(): void {
    this.sheetResource.reload();
  }

  public setView(view: SheetView): void {
    this.view.set(view);
    this.rawSelection.set(null);
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
    const before = new Set(this.index().tables.keys());
    const sheet = await this.run(() => this.api.addTable(sheetId, tableTemplateId));
    const added = sheet?.tables.find((table) => !before.has(table.id));
    if (added) {
      this.rawSelection.set({ tableId: added.id, sectionId: null, rowId: null });
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
    const before = sectionIds(this.index());
    const sheet = await this.run(() =>
      this.api.addSection(tableId, templateSectionId, parentSheetSectionId),
    );
    if (sheet) {
      const after = buildSheetIndex(sheet);
      const added = [...after.sections.keys()].find((id) => !before.has(id));
      if (added !== undefined) {
        this.rawSelection.set({ tableId, sectionId: added, rowId: null });
      }
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

  public async addRow(sectionId: number, templateRowId: number): Promise<void> {
    const tableId = this.index().sectionTable.get(sectionId);
    const before = rowIds(this.index());
    const sheet = await this.run(() => this.api.addRow(sectionId, templateRowId));
    if (sheet && tableId !== undefined) {
      const added = [...buildSheetIndex(sheet).rows.keys()].find((id) => !before.has(id));
      this.rawSelection.set({ tableId, sectionId, rowId: added ?? null });
    }
  }

  public async lockRow(rowId: number): Promise<void> {
    await this.run(() => this.api.lockRow(rowId));
  }

  /**
   * The user moved into a cell: take the row's lock if nobody has it, so what they type is saved to
   * their own draft. A row they already hold, or someone else holds, is left alone.
   */
  public beginEditing(rowId: number): void {
    const row = this.index().rows.get(rowId);
    if (!this.canEdit() || row === undefined || row.lock !== null || this.lockRequests.has(rowId)) {
      return;
    }
    this.lockRequests.add(rowId);
    this.selectRow(rowId);
    void this.lockRow(rowId).finally(() => this.lockRequests.delete(rowId));
  }

  public async saveValues(rowId: number, values: CellValueRequest[]): Promise<void> {
    await this.run(() => this.api.saveValues(rowId, values));
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

  private run(change: () => Promise<Sheet>): Promise<Sheet | null> {
    const task = this.tail.then(() => this.execute(change));
    this.tail = task;
    return task;
  }

  private async execute(change: () => Promise<Sheet>): Promise<Sheet | null> {
    this.inFlight.update((count) => count + 1);
    this.error.set(null);
    try {
      const sheet = await change();
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
