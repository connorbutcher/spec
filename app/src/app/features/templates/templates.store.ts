import { httpResource } from '@angular/common/http';
import { computed, inject, Injectable, linkedSignal, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { SheetType } from '../../core/models/sheet-type.model';
import { apiErrorMessage } from './api-error-message';
import { CellType } from './models/cell-type.model';
import { SaveCellTypeRequest } from './models/save-cell-type-request.model';
import { TableTemplateSummary } from './models/table-template-summary.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateIndex } from './models/template-index.model';
import { TemplateLayout } from './models/template-layout.model';
import { TemplateOrientation } from './models/template-orientation';
import { UpdateTemplateCellRequest } from './models/update-template-cell-request.model';
import { UpdateTemplateSectionRequest } from './models/update-template-section-request.model';
import { findRouteParam } from './route-param.util';
import { addedIds, buildTemplateIndex } from './template-index.util';
import { layoutTemplate } from './template-layout.util';
import { TemplatesApi } from './templates-api';

/**
 * State for the templates screen: sheet types, template list and cell types (resources), the open
 * template and its layout, and every change the designer makes. Provided by the templates page.
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

  public readonly cellTypes = computed<CellType[]>(() =>
    this.cellTypesResource.hasValue() ? this.cellTypesResource.value() : [],
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
    const id = findRouteParam(this.router.routerState.snapshot.root, 'templateId');
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

  public readonly index = computed<TemplateIndex>(() => buildTemplateIndex(this.template()));

  public readonly layout = computed<TemplateLayout | null>(() => {
    const template = this.template();
    return template && template.sections.length > 0 ? layoutTemplate(template) : null;
  });

  /** True while a change is being saved. */
  public readonly isSaving = computed(() => this.pendingSaves() > 0);

  /** The last save's error, shown until dismissed or the next save. */
  public readonly error = signal<string | null>(null);

  private readonly router = inject(Router);
  private readonly api = inject(TemplatesApi);

  private readonly navigationEnd = toSignal(
    this.router.events.pipe(filter((event) => event instanceof NavigationEnd)),
  );

  private readonly sheetTypesResource = httpResource<SheetType[]>(() => '/api/sheet-types');
  private readonly summariesResource = httpResource<TableTemplateSummary[]>(
    () => '/api/table-templates',
  );
  private readonly cellTypesResource = httpResource<CellType[]>(() => '/api/cell-types');

  private readonly templateResource = httpResource<TableTemplate>(() => {
    const id = this.selectedTemplateId();
    if (id === null) {
      return undefined;
    }
    const version = this.viewedVersion();
    return version === null
      ? `/api/table-templates/${id}`
      : `/api/table-templates/${id}?version=${version}`;
  });

  private readonly cellTypesById = computed(
    () => new Map(this.cellTypes().map((cellType) => [cellType.id, cellType])),
  );

  private readonly pendingSaves = signal(0);

  public reloadList(): void {
    this.sheetTypesResource.reload();
    this.summariesResource.reload();
    this.cellTypesResource.reload();
  }

  public reloadTemplate(): void {
    this.templateResource.reload();
  }

  public dismissError(): void {
    this.error.set(null);
  }

  public templatesFor(sheetTypeId: number): TableTemplateSummary[] {
    return this.summaries()
      .filter((summary) => summary.sheetTypeId === sheetTypeId)
      .sort((a, b) => a.displayOrder - b.displayOrder);
  }

  public cellType(id: number): CellType | undefined {
    return this.cellTypesById().get(id);
  }

  public async createTemplate(sheetTypeId: number): Promise<void> {
    const name = nextName(
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

  public async updateTemplate(name: string, orientation: TemplateOrientation): Promise<void> {
    const template = this.template();
    if (!template) {
      return;
    }
    const updated = await this.save(() => this.api.updateTemplate(template.id, name, orientation));
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
      this.cellTypesResource.reload();
      await this.router.navigate(['/templates']);
    }
  }

  /** Adds a section at the top level or inside `parentId`. Returns the new section's id. */
  public async addSection(parentId: number | null): Promise<number | null> {
    const template = this.template();
    if (!template) {
      return null;
    }
    const siblings =
      parentId === null
        ? template.sections
        : (this.index().sections.get(parentId)?.section.sections ?? []);
    const name = nextName(
      'Section',
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
      role: section.role,
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

  /** Adds a row to the end of a section. Returns the new row's id. */
  public async addRow(sectionId: number): Promise<number | null> {
    return this.changeTemplate(() => this.api.createRow(sectionId), 'rows');
  }

  public async moveRow(id: number, position: number): Promise<void> {
    await this.changeTemplate(() => this.api.moveRow(id, position));
  }

  public async deleteRow(id: number): Promise<boolean> {
    return (await this.changeTemplate(() => this.api.deleteRow(id))) !== null;
  }

  /** Adds a cell to the end of a row. Returns the new cell's id. */
  public async addCell(rowId: number): Promise<number | null> {
    return this.changeTemplate(() => this.api.createCell(rowId), 'cells');
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

  public async deleteCell(id: number): Promise<boolean> {
    return (await this.changeTemplate(() => this.api.deleteCell(id))) !== null;
  }

  /** Adds a Text cell type with an unused name. Returns its id. */
  public async createCellType(): Promise<number | null> {
    const name = nextName(
      'New cell type',
      this.cellTypes().map((cellType) => cellType.name),
    );
    const created = await this.save(() =>
      this.api.createCellType({
        name,
        kind: 'Text',
        description: null,
        maxLength: null,
        decimalPlaces: null,
        minValue: null,
        maxValue: null,
        unit: null,
        options: [],
      }),
    );
    if (!created) {
      return null;
    }
    this.cellTypesResource.update((cellTypes) => [...(cellTypes ?? []), created]);
    return created.id;
  }

  /** Changes some of a cell type's settings, keeping the rest. */
  public async updateCellType(id: number, changes: Partial<SaveCellTypeRequest>): Promise<void> {
    const cellType = this.cellType(id);
    if (!cellType) {
      return;
    }
    const request: SaveCellTypeRequest = {
      name: cellType.name,
      kind: cellType.kind,
      description: cellType.description,
      maxLength: cellType.maxLength,
      decimalPlaces: cellType.decimalPlaces,
      minValue: cellType.minValue,
      maxValue: cellType.maxValue,
      unit: cellType.unit,
      options: cellType.options.map((option) => option.value),
      ...changes,
    };
    const updated = await this.save(() => this.api.updateCellType(id, request));
    if (updated) {
      this.cellTypesResource.update((cellTypes) =>
        (cellTypes ?? []).map((candidate) => (candidate.id === id ? updated : candidate)),
      );
    }
  }

  public async deleteCellType(id: number): Promise<boolean> {
    const deleted = await this.save(async () => {
      await this.api.deleteCellType(id);
      return true;
    });
    if (deleted) {
      this.cellTypesResource.update((cellTypes) =>
        (cellTypes ?? []).filter((cellType) => cellType.id !== id),
      );
    }
    return deleted === true;
  }

  /**
   * Runs a template change and swaps in the returned template. With `added`, returns the id of the
   * section, row or cell the change created; otherwise returns 0 on success. Null means it failed.
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
    this.cellTypesResource.reload();
    return added ? (addedIds(before[added], this.index()[added])[0] ?? null) : 0;
  }

  private async save<T>(action: () => Promise<T>): Promise<T | null> {
    this.error.set(null);
    this.pendingSaves.update((count) => count + 1);
    try {
      return await action();
    } catch (error) {
      this.error.set(apiErrorMessage(error));
      return null;
    } finally {
      this.pendingSaves.update((count) => count - 1);
    }
  }
}

/** `base`, or `base 2`, `base 3`... whichever isn't taken. With `numbered`, starts at `base 1`. */
function nextName(base: string, taken: string[], numbered = false): string {
  const names = new Set(taken.map((name) => name.toLowerCase()));
  if (!numbered && !names.has(base.toLowerCase())) {
    return base;
  }
  for (let number = numbered ? 1 : 2; ; number++) {
    const candidate = `${base} ${number}`;
    if (!names.has(candidate.toLowerCase())) {
      return candidate;
    }
  }
}
