import { httpResource } from '@angular/common/http';
import { computed, inject, Injectable, linkedSignal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { SheetType } from '../../core/models/sheet-type.model';
import { TableTemplateSummary } from './models/table-template-summary.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateIndex } from './models/template-index.model';
import { TemplateLayout } from './models/template-layout.model';
import { TemplateOrientation } from './models/template-orientation';
import { UpdateTemplateCellOverridesRequest } from './models/update-template-cell-overrides-request.model';
import { UpdateTemplateCellRequest } from './models/update-template-cell-request.model';
import { UpdateTemplateColumnBlockRequest } from './models/update-template-column-block-request.model';
import { UpdateTemplateSectionRequest } from './models/update-template-section-request.model';
import { RouteParamUtil } from '../../core/route-param.util';
import { TemplateIndexUtil } from './template-index.util';
import { TemplateLayoutUtil } from './template-layout.util';
import { CellTypesStore } from './cell-types.store';
import { TemplateNamesUtil } from './template-names.util';
import { TemplateSaves } from './template-saves';
import { TemplatesApi } from './templates-api';

/**
 * State for the templates screen: sheet types and the template list (resources), the open
 * template and its layout, and every change the designer makes to templates. Cell types are in
 * `CellTypesStore`, and both save through `TemplateSaves`. Provided by the templates page.
 *
 * Changes save straight away. Template changes return the whole template, which replaces the open
 * copy; creates return the new item's id so the panel can open it.
 */
@Injectable()

export class TemplatesStore {
  public readonly sheetTypes = computed<SheetType[]>(() =>
    this.sheetTypesResource.hasValue()
      ? [...this.sheetTypesResource.value()].sort((a, b) => a.displayOrder - b.displayOrder)
      : [],
  );

  public readonly summaries = computed<TableTemplateSummary[]>(() =>
    this.summariesResource.hasValue() ? this.summariesResource.value() : [],
  );

  public readonly listIsLoading = computed(
    () => this.sheetTypesResource.isLoading() || this.summariesResource.isLoading(),
  );

  public readonly listHasError = computed(
    () =>
      this.sheetTypesResource.status() === 'error' || this.summariesResource.status() === 'error',
  );

  /** The template id in the URL (`/templates/:templateId`), or null on `/templates`. */
  public readonly selectedTemplateId = computed<number | null>(() => {
    this.navigationEnd();
    const id = RouteParamUtil.findRouteParam(this.router.routerState.snapshot.root, 'templateId');
    return id === null ? null : Number(id);
  });

  /** The open template, once it has loaded. */
  public readonly template = computed<TableTemplate | null>(() => {
    const id = this.selectedTemplateId();
    if (id === null || !this.templateResource.hasValue()) {
      return null;
    }
    const template = this.templateResource.value();
    return template?.id === id ? template : null;
  });

  /** Whether the open version can change: the latest version, while no sheet uses it. */
  public readonly canEdit = computed(() => this.template()?.isEditable ?? false);

  /**
   * The version being looked at, or null for the latest. Goes back to the latest whenever a different
   * table is opened.
   */
  public readonly viewedVersion = linkedSignal<number | null, number | null>({
    source: () => this.selectedTemplateId(),
    computation: () => null,
  });

  public readonly templateIsLoading = computed(() => this.templateResource.isLoading());
  public readonly templateHasError = computed(() => this.templateResource.status() === 'error');

  public readonly index = computed<TemplateIndex>(() => TemplateIndexUtil.buildTemplateIndex(this.template()));

  public readonly layout = computed<TemplateLayout | null>(() => {
    const template = this.template();
    return template && template.sections.length > 0 ? TemplateLayoutUtil.layoutTemplate(template) : null;
  });

  /** True while a change to a template or a cell type is being saved. */
  public readonly isSaving = computed(() => this.saves.isSaving());

  /** The last save's error, shown until dismissed or the next save. */
  public readonly error = computed(() => this.saves.error());

  private readonly router = inject(Router);
  private readonly api = inject(TemplatesApi);
  private readonly saves = inject(TemplateSaves);
  private readonly cellTypes = inject(CellTypesStore);

  private readonly navigationEnd = toSignal(
    this.router.events.pipe(filter((event) => event instanceof NavigationEnd)),
  );

  private readonly sheetTypesResource = httpResource<SheetType[]>(() => '/api/sheet-types');
  private readonly summariesResource = httpResource<TableTemplateSummary[]>(
    () => '/api/table-templates',
  );
  private readonly templateResource = httpResource<TableTemplate>(() => {
    const id = this.selectedTemplateId();
    if (id === null) {
      return undefined;
    }
    // A cell type changing kind drops its cells' overrides on the server, so the template is read again.
    this.cellTypes.overridesCleared();
    const version = this.viewedVersion();
    return version === null
      ? `/api/table-templates/${id}`
      : `/api/table-templates/${id}?version=${version}`;
  });

  /** The last cell override save, which the next one waits for. */
  private overrideSaves: Promise<void> = Promise.resolve();

  public reloadList(): void {
    this.sheetTypesResource.reload();
    this.summariesResource.reload();
    this.cellTypes.reload();
  }

  public reloadTemplate(): void {
    this.templateResource.reload();
  }

  public dismissError(): void {
    this.saves.dismissError();
  }

  public templatesFor(sheetTypeId: number): TableTemplateSummary[] {
    return this.summaries()
      .filter((summary) => summary.sheetTypeId === sheetTypeId)
      .sort((a, b) => a.displayOrder - b.displayOrder);
  }

  public async createTemplate(sheetTypeId: number): Promise<void> {
    const name = TemplateNamesUtil.nextName(
      'New table',
      this.templatesFor(sheetTypeId).map((summary) => summary.name),
    );
    const created = await this.save(() => this.api.createTemplate(sheetTypeId, name, 'Horizontal'));
    if (created) {
      this.templateResource.set(created);
      this.summariesResource.reload();
      await this.router.navigate(['/templates', created.id]);
    }
  }

  public async updateTemplate(
    name: string,
    orientation: TemplateOrientation,
    stickyColumnCount?: number,
  ): Promise<void> {
    const template = this.template();
    if (!template) {
      return;
    }
    const updated = await this.save(() =>
      this.api.updateTemplate(
        template.id,
        name,
        orientation,
        stickyColumnCount ?? template.stickyColumnCount,
      ),
    );
    if (updated) {
      this.templateResource.set(updated);
      this.summariesResource.reload();
    }
  }

  /** Shows an older version (read-only), or the latest with null. */
  public showVersion(versionNumber: number | null): void {
    const latest = this.template()?.versions.at(-1)?.versionNumber;
    this.viewedVersion.set(versionNumber === latest ? null : versionNumber);
  }

  /** Copies the latest version into a new editable one and opens it. */
  public async createVersion(): Promise<boolean> {
    const template = this.template();
    if (!template) {
      return false;
    }
    const created = await this.save(() => this.api.createVersion(template.id));
    if (!created) {
      return false;
    }
    this.viewedVersion.set(null);
    this.templateResource.set(created);
    this.summariesResource.reload();
    return true;
  }

  public async deleteTemplate(): Promise<void> {
    const template = this.template();
    if (!template) {
      return;
    }
    const deleted = await this.save(async () => {
      await this.api.deleteTemplate(template.id);
      return true;
    });
    if (deleted) {
      this.summariesResource.reload();
      this.cellTypes.reload();
      await this.router.navigate(['/templates']);
    }
  }

  /**
   * Adds an addable section at the top level, or a sub-section inside `parentId`. (The header comes
   * with the table.) Returns the new section's id.
   */
  public async addSection(parentId: number | null): Promise<number | null> {
    const template = this.template();
    if (!template) {
      return null;
    }
    const siblings =
      parentId === null
        ? template.sections
        : (this.index().sections.get(parentId)?.section.sections ?? []);
    const name = TemplateNamesUtil.nextName(
      parentId === null ? 'Section' : 'Sub-section',
      siblings.map((section) => section.name),
      true,
    );
    return this.changeTemplate(
      () => this.api.createSection(template.versionId, parentId, name),
      'sections',
    );
  }

  /** Changes some of a section's settings, keeping the rest. */
  public async updateSection(
    id: number,
    changes: Partial<UpdateTemplateSectionRequest>,
  ): Promise<void> {
    const section = this.index().sections.get(id)?.section;
    if (!section) {
      return;
    }
    const request: UpdateTemplateSectionRequest = {
      name: section.name,
      minInstances: section.minInstances,
      maxInstances: section.maxInstances,
      initialInstances: section.initialInstances,
      ...changes,
    };
    await this.changeTemplate(() => this.api.updateSection(id, request));
  }

  public async moveSection(id: number, position: number): Promise<void> {
    await this.changeTemplate(() => this.api.moveSection(id, position));
  }

  public async deleteSection(id: number): Promise<boolean> {
    return (await this.changeTemplate(() => this.api.deleteSection(id))) !== null;
  }

  /** Adds a column block after the others (horizontal tables only). Returns the new block's id. */
  public async addColumnBlock(): Promise<number | null> {
    const template = this.template();
    if (!template) {
      return null;
    }
    const name = TemplateNamesUtil.nextName(
      'Column block',
      template.columnBlocks.map((block) => block.name),
      true,
    );
    return this.changeTemplate(
      () => this.api.createColumnBlock(template.versionId, name),
      'columnBlocks',
    );
  }

  /** Changes some of a column block's settings, keeping the rest. */
  public async updateColumnBlock(
    id: number,
    changes: Partial<UpdateTemplateColumnBlockRequest>,
  ): Promise<void> {
    const block = this.index().columnBlocks.get(id)?.block;
    if (!block) {
      return;
    }
    const request: UpdateTemplateColumnBlockRequest = {
      name: block.name,
      minInstances: block.minInstances,
      maxInstances: block.maxInstances,
      initialInstances: block.initialInstances,
      stickyColumnCount: block.stickyColumnCount,
      ...changes,
    };
    await this.changeTemplate(() => this.api.updateColumnBlock(id, request));
  }

  public async moveColumnBlock(id: number, position: number): Promise<void> {
    await this.changeTemplate(() => this.api.moveColumnBlock(id, position));
  }

  public async deleteColumnBlock(id: number): Promise<boolean> {
    return (await this.changeTemplate(() => this.api.deleteColumnBlock(id))) !== null;
  }

  /**
   * Adds a row to a section at 1-based `position` (default: the end), copying the columns of
   * `copyFromRowId` (default: the last row). Returns the new row's id.
   */
  public async addRow(
    sectionId: number,
    position: number | null = null,
    copyFromRowId: number | null = null,
  ): Promise<number | null> {
    return this.changeTemplate(
      () => this.api.createRow(sectionId, position, copyFromRowId),
      'rows',
    );
  }

  public async moveRow(id: number, position: number): Promise<void> {
    await this.changeTemplate(() => this.api.moveRow(id, position));
  }

  public async deleteRow(id: number): Promise<boolean> {
    return (await this.changeTemplate(() => this.api.deleteRow(id))) !== null;
  }

  /**
   * Adds a cell to a row at `column`, moving the cells from there on one column along (default: after
   * the last cell), among the row's own cells or a column block's. Returns the new cell's id.
   */
  public async addCell(
    rowId: number,
    column: number | null = null,
    columnBlockId: number | null = null,
  ): Promise<number | null> {
    return this.changeTemplate(() => this.api.createCell(rowId, column, columnBlockId), 'cells');
  }

  /** Changes some of a cell's settings, keeping the rest. */
  public async updateCell(id: number, changes: Partial<UpdateTemplateCellRequest>): Promise<void> {
    const cell = this.index().cells.get(id)?.cell;
    if (!cell) {
      return;
    }
    const request: UpdateTemplateCellRequest = {
      cellTypeId: cell.cellTypeId,
      column: cell.column,
      rowSpan: cell.rowSpan,
      columnSpan: cell.columnSpan,
      caption: cell.caption,
      isRequired: cell.isRequired,
      ...changes,
    };
    await this.changeTemplate(() => this.api.updateCell(id, request));
  }

  /** Sets or removes the name other applications look a cell up by. */
  public async updateCellLookupKey(id: number, lookupKey: string | null): Promise<void> {
    await this.changeTemplate(() => this.api.updateCellLookupKey(id, lookupKey));
  }

  public async deleteCell(id: number): Promise<boolean> {
    return (await this.changeTemplate(() => this.api.deleteCell(id))) !== null;
  }

  /** Changes what a cell overrides from its cell type's defaults, keeping the rest. */
  public updateCellOverrides(
    id: number,
    changes: Partial<UpdateTemplateCellOverridesRequest>,
  ): Promise<void> {
    // Quick clicks (bold, then align) each build on the result of the one before, so run them in turn.
    const next = this.overrideSaves.then(async () => {
      const cell = this.index().cells.get(id)?.cell;
      if (!cell) {
        return;
      }
      const request: UpdateTemplateCellOverridesRequest = {
        configurationOverride: cell.configurationOverride,
        styleOverride: cell.styleOverride,
        ...changes,
      };
      await this.changeTemplate(() => this.api.updateCellOverrides(id, request));
    });
    this.overrideSaves = next;
    return next;
  }

  /**
   * Runs a template change and swaps in the returned template. With `added`, returns the id of the
   * section, column block, row or cell the change created; otherwise returns 0 on success. Null means it failed.
   */
  private async changeTemplate(
    change: () => Promise<TableTemplate>,
    added?: keyof TemplateIndex,
  ): Promise<number | null> {
    const before = this.index();
    const updated = await this.save(change);
    if (!updated) {
      return null;
    }
    this.templateResource.set(updated);
    // Cell counts per type change as cells come and go.
    this.cellTypes.reload();
    return added ? (TemplateIndexUtil.addedIds(before[added], this.index()[added])[0] ?? null) : 0;
  }

  private save<T>(action: () => Promise<T>): Promise<T | null> {
    return this.saves.run(action);
  }
}
