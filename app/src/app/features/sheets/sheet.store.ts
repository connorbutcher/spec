import { httpResource } from '@angular/common/http';
import {
  computed,
  effect,
  inject,
  Injectable,
  linkedSignal,
  Signal,
  signal,
  untracked,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';
import { KeyedFlags } from '../../shared/keyed-flags';
import { ReuseUnchangedUtil } from '../../shared/reuse-unchanged.util';
import { ApiErrorMessage } from '../templates/api-error-message';
import { CellType } from '../templates/models/cell-type.model';
import { CellSettingsRequest } from './models/cell-settings-request.model';
import { CellValueRequest } from './models/cell-value-request.model';

import { SheetLock } from './models/sheet-lock.model';
import { PublishScope } from './models/publish-scope';
import { SheetChange } from './models/sheet-change.model';
import { SheetIndex } from './models/sheet-index.model';
import { SheetSelection } from './models/sheet-selection.model';
import { SheetTarget } from './models/sheet-target.model';
import { SheetVersionSummary } from './models/sheet-version-summary.model';
import { SheetView } from './models/sheet-view.model';
import { SheetResponse } from './models/sheet-response.model';
import { SheetRowsChange } from './models/sheet-rows-change.model';
import { Sheet } from './models/sheet.model';
import { RowCheckoutStore } from './checkouts/row-checkout.store';
import { RowCheckoutUtil } from './checkouts/row-checkout.util';
import { SheetIndexUtil } from './layout/sheet-index.util';
import { SheetResponseUtil } from './data/sheet-response.util';
import { SheetRowsMergeUtil } from './data/sheet-rows-merge.util';
import { SheetUrlUtil } from './anchors/sheet-url.util';
import { SheetsApi } from './sheets-api';

/**
 * State for the sheet screen: the sheet being looked at (a resource, live or at a version or date), the
 * cell types its cells use, what is selected, and every change the user makes. Provided by the sheet page.
 *
 * Changes save straight away. Saving a row's values or cell settings returns just that row (and what
 * follows from it), which is put into the open copy; every other change returns the refreshed live
 * sheet, which replaces it. Changes are sent one at a time, in the order they were made, and the quiet
 * reads that follow other people's changes wait their turn in the same queue, so an older read never
 * lands on top of a newer save. A failed change shows its reason and reloads the sheet, so the screen
 * goes back to what the server has.
 *
 * A refreshed sheet keeps the objects of everything that did not change (see `reuseUnchanged`), so a
 * change only re-renders the tables, rows and cells it touched.
 */
const NOBODY: ReadonlyMap<number, SheetLock> = new Map();

@Injectable()

export class SheetStore {
  /**
   * Which moment of the sheet is shown. Goes back to live whenever a different sheet is opened, unless the
   * address asks to review a submission (`?review=`), as the Approvals screen does.
   */
  public readonly view = linkedSignal<{ target: SheetTarget | null; review: number | null }, SheetView>({
    source: () => ({ target: this.target(), review: this.reviewParam() }),
    computation: ({ review }) => (review === null ? {} : { submission: review }),
  });

  /** Mark everything that changed after this version; null leaves the tables clean. */
  public readonly changesSince = linkedSignal<SheetTarget | null, number | null>({
    source: () => this.target(),
    computation: () => null,
  });

  /**
   * The open sheet, once its cell types have arrived too: without them no cell knows what it is, so
   * the sheet isn't shown until both are in.
   *
   * A row someone has clicked into, but not changed, is theirs without anything being saved, so the
   * server's sheet doesn't show it. Those rows are given their lock here (see `RowCheckoutStore`), and
   * everything downstream treats them like any other locked row.
   */
  public readonly sheet = computed<Sheet | null>(() => {
    const loaded = this.loadedSheet();
    return loaded === null ? null : RowCheckoutUtil.withRowCheckouts(loaded, this.rowCheckouts?.locks() ?? NOBODY);
  });

  public readonly isLoading = computed(
    () => this.sheetResource.isLoading() || this.cellTypesResource.isLoading(),
  );

  public readonly hasError = computed(
    () => this.sheetResource.status() === 'error' || this.cellTypesResource.status() === 'error',
  );

  /** Only the live view can be changed; a version or date is read-only. */
  public readonly canEdit = computed(() => this.sheet()?.isLive === true);

  public readonly index = computed<SheetIndex>(() => SheetIndexUtil.buildSheetIndex(this.sheet()));

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

  /**
   * The cell whose control is on screen. Every other cell that can be edited shows its plain value
   * until it is focused, so a sheet doesn't build a control for each of its cells.
   */
  public readonly editingCellId = computed(() => this.editingCell());

  /** The reason the last change failed, until it's dismissed or another change is made. */
  public readonly error = signal<string | null>(null);

  public readonly isBusy = computed(() => this.inFlight() > 0);

  /** What the publish that is running was asked to take; null when none is running. */
  public readonly publishing = signal<PublishScope | null>(null);

  private readonly api = inject(SheetsApi);
  /** Absent where the sheet is shown without its live side, as in tests of the store alone. */
  private readonly rowCheckouts = inject(RowCheckoutStore, { optional: true });
  private readonly route = inject(ActivatedRoute);
  private readonly rawSelection = signal<SheetSelection | null>(null);
  private readonly editingCell = signal<number | null>(null);

  /**
   * The selected row and the cell being edited, as a flag per row and per cell. A sheet can have
   * thousands of cells; with a flag each, a click changes what two rows or two cells show, instead of
   * making every cell on the sheet compare itself with the selection.
   */
  private readonly selectedRows = new KeyedFlags();
  private readonly editingCells = new KeyedFlags();

  private readonly inFlight = signal(0);
  private tail: Promise<unknown> = Promise.resolve();

  private readonly target = toSignal(
    this.route.paramMap.pipe(
      map((params) => SheetUrlUtil.sheetTarget(params.get('phaseId'), params.get('sheetTypeId'))),
    ),
    { initialValue: null },
  );

  /** The submission the address asks to review, or null. */
  private readonly reviewParam = toSignal(
    this.route.queryParamMap.pipe(map((params) => SheetUrlUtil.reviewedSubmission(params.get('review')))),
    { initialValue: null },
  );

  /** Set for a quiet reload of the sheet: that read keeps the objects of everything that didn't change. */
  private keepUnchangedOnNextRead = false;

  /** Whether a quiet read of the whole sheet is waiting in the queue. */
  private fullReadQueued = false;

  /** The rows a quiet read waiting in the queue will read; null when none is waiting. */
  private queuedRowIds: Set<number> | null = null;

  private readonly sheetResource = httpResource<Sheet>(
    () => {
      const target = this.target();
      return target === null ? undefined : SheetUrlUtil.sheetUrl(target, this.view());
    },
    { parse: (body) => this.received(SheetResponseUtil.withTemplateCells(body as SheetResponse)) },
  );

  private readonly cellTypesResource = httpResource<CellType[]>(() => '/api/cell-types');

  /** The sheet as the server sent it, before the rows people are in are marked. */
  private readonly loadedSheet = computed<Sheet | null>(() =>
    this.sheetResource.hasValue() && this.cellTypesResource.hasValue()
      ? this.sheetResource.value()
      : null,
  );

  constructor() {
    // The selection can also end because its row left the sheet, so its flags follow what it ends up as.
    effect(() => {
      const rowId = this.selection()?.rowId ?? null;
      untracked(() => this.selectedRows.set(rowId));
    });
  }

  public reload(): void {
    this.sheetResource.reload();
    if (this.cellTypesResource.status() === 'error') {
      this.cellTypesResource.reload();
    }
  }

  /**
   * Someone else changed the sheet (a version published, a table or section taken): reads it again
   * without disturbing the user, after the user's own changes already queued. A read already waiting
   * covers this one. Everything that didn't change keeps its object, so a cell being typed into keeps
   * what was typed.
   */
  public refresh(): void {
    if (this.fullReadQueued) {
      return;
    }
    this.fullReadQueued = true;
    this.enqueueRead(async () => {
      this.fullReadQueued = false;
      const open = this.openSheet();
      if (open === null) {
        return;
      }
      if (!open.isLive) {
        // A past view never changes; only its list of versions can grow, which a reload picks up.
        this.keepUnchangedOnNextRead = true;
        this.sheetResource.reload();
        return;
      }
      this.applyQuietly(await this.api.live(open.id));
    });
  }

  /**
   * Someone else checked rows out or released them: reads only those rows again, like `refresh` does
   * the whole sheet. Rows named while a read is waiting join it.
   */
  public refreshRows(rowIds: readonly number[]): void {
    if (this.queuedRowIds !== null) {
      rowIds.forEach((rowId) => this.queuedRowIds?.add(rowId));
      return;
    }
    const queued = new Set(rowIds);
    this.queuedRowIds = queued;
    this.enqueueRead(async () => {
      this.queuedRowIds = null;
      const open = this.openSheet();
      if (open === null || !open.isLive) {
        return;
      }
      const asked = [...queued];
      this.applyQuietly(await this.withRows(this.api.rows(open.id, asked), asked));
    });
  }

  public setView(view: SheetView): void {
    const since = this.changesSince();
    if (since !== null && view.version !== undefined && since >= view.version) {
      // Nothing in a version can have changed since that version or a later one.
      this.changesSince.set(null);
    }
    this.view.set(view);
    this.rawSelection.set(null);
    this.setEditing(null);
  }

  /** Whether the row is the selected one. One flag per row: see `selectedRows`. */
  public isRowSelected(rowId: number): Signal<boolean> {
    return this.selectedRows.flag(rowId);
  }

  /** Whether the cell's control is on screen. One flag per cell: see `editingCells`. */
  public isEditing(cellId: number): Signal<boolean> {
    return this.editingCells.flag(cellId);
  }

  /** Puts a cell's control on screen, in place of whichever cell had one. */
  public edit(cellId: number): void {
    this.setEditing(cellId);
  }

  /** The user left a cell: it goes back to its plain value, unless another cell has taken over since. */
  public stopEditing(cellId: number): void {
    if (this.editingCell() === cellId) {
      this.setEditing(null);
    }
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
    const added = sheet ? SheetIndexUtil.addedId(before, this.index().tables) : undefined;
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
    const added = sheet ? SheetIndexUtil.addedId(before, this.index().sections) : undefined;
    if (added !== undefined) {
      this.rawSelection.set({ tableId, sectionId: added, rowId: null });
    }
  }

  public async moveSection(sectionId: number, step: -1 | 1): Promise<void> {
    const siblings = SheetIndexUtil.sectionSiblings(this.index(), sectionId);
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
      () => this.withRows(this.api.saveValues(rowId, values), [rowId]),
      values.map((value) => value.sheetCellId),
    );
  }

  public async saveCellSettings(rowId: number, settings: CellSettingsRequest[]): Promise<void> {
    await this.run(
      () => this.withRows(this.api.saveCellSettings(rowId, settings), [rowId]),
      settings.map((entry) => entry.sheetCellId),
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

  /**
   * Submits changes for publishing to the chosen approvers. They stay on the sheet, frozen and marked as
   * waiting for approval, until every approver approves (then they are the next version) or one rejects.
   */
  public async publish(
    note: string | null,
    scope: PublishScope = 'Mine',
    approverUserIds: readonly number[] = [],
  ): Promise<boolean> {
    const sheetId = this.sheet()?.id;
    if (sheetId === undefined) {
      return false;
    }
    return (await this.run(() => this.publishNow(sheetId, note, scope, approverUserIds))) !== null;
  }

  /** Submits, with `publishing` saying what is going out for exactly as long as the call takes. */
  private async publishNow(
    sheetId: number,
    note: string | null,
    scope: PublishScope,
    approverUserIds: readonly number[],
  ): Promise<Sheet> {
    this.publishing.set(scope);
    try {
      return await this.api.publish(sheetId, note, scope, approverUserIds);
    } finally {
      this.publishing.set(null);
    }
  }

  private setEditing(cellId: number | null): void {
    this.editingCell.set(cellId);
    this.editingCells.set(cellId);
  }

  /** The sheet as last loaded, without the rows people are in marked. */
  private openSheet(): Sheet | null {
    return untracked(() => (this.sheetResource.hasValue() ? this.sheetResource.value() : null));
  }

  /**
   * The open sheet with the rows a call answers with put in. When they can't be (a version was published
   * since, or a row isn't on this copy), the whole live sheet is read instead.
   */
  private async withRows(answer: Promise<SheetRowsChange>, rowIds: readonly number[]): Promise<Sheet> {
    const change = await answer;
    const open = this.openSheet();
    const merged = open === null ? null : SheetRowsMergeUtil.merge(open, change, rowIds);
    return merged ?? this.api.live(change.sheetId);
  }

  /** Queues a quiet read behind the user's changes. One that fails falls back to reloading the sheet. */
  private enqueueRead(read: () => Promise<void>): void {
    this.tail = this.tail.then(read).catch(() => {
      this.keepUnchangedOnNextRead = true;
      this.sheetResource.reload();
    });
  }

  /** Shows a sheet a quiet read brought, keeping every object that didn't change, if it is still the one open. */
  private applyQuietly(next: Sheet): void {
    const open = this.openSheet();
    if (open?.id === next.id && open.isLive) {
      this.sheetResource.set(ReuseUnchangedUtil.reuseUnchanged(open, next));
    }
  }

  /** A sheet as it arrives from a read. Only a quiet `refresh` keeps the open copy's unchanged objects. */
  private received(sheet: Sheet): Sheet {
    const keep = this.keepUnchangedOnNextRead;
    this.keepUnchangedOnNextRead = false;
    const open = untracked(() =>
      this.sheetResource.hasValue() ? this.sheetResource.value() : null,
    );
    return keep && open?.id === sheet.id ? ReuseUnchangedUtil.reuseUnchanged(open, sheet) : sheet;
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
      const saved = SheetIndexUtil.cellsById(refreshed, savedCellIds);
      const sheet = ReuseUnchangedUtil.reuseUnchanged(this.sheet(), refreshed, (part) => saved.has(part));
      this.sheetResource.set(sheet);
      return sheet;
    } catch (failure) {
      this.error.set(ApiErrorMessage.apiErrorMessage(failure));
      this.sheetResource.reload();
      return null;
    } finally {
      this.inFlight.update((count) => count - 1);
    }
  }
}
