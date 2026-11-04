import { httpResource } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { CellType } from './models/cell-type.model';
import { SaveCellTypeRequest } from './models/save-cell-type-request.model';
import { TemplateNamesUtil } from './template-names.util';
import { TemplateSaves } from './template-saves';
import { TemplatesApi } from './templates-api';

/**
 * The cell types on the templates screen, with how many template cells use each, and the changes made to
 * them. Provided by the templates page beside `TemplatesStore`, which reads the types its cells use here.
 */
@Injectable()
export class CellTypesStore {
  public readonly cellTypes = computed<CellType[]>(() =>
    this.resource.hasValue() ? this.resource.value() : [],
  );

  /**
   * Counts the cell type changes that dropped the configuration overrides of the cells using the type
   * (a change of kind). The open template is read again whenever it moves.
   */
  public readonly overridesCleared = computed(() => this.clearedOverrides());

  private readonly api = inject(TemplatesApi);
  private readonly saves = inject(TemplateSaves);
  private readonly resource = httpResource<CellType[]>(() => '/api/cell-types');
  private readonly clearedOverrides = signal(0);

  private readonly byId = computed(
    () => new Map(this.cellTypes().map((cellType) => [cellType.id, cellType])),
  );

  public cellType(id: number): CellType | undefined {
    return this.byId().get(id);
  }

  /** Reads the types again, as when cells come and go and the counts change. */
  public reload(): void {
    this.resource.reload();
  }

  /** Adds a Text cell type with an unused name. Returns its id. */
  public async createCellType(): Promise<number | null> {
    const name = TemplateNamesUtil.nextName(
      'New cell type',
      this.cellTypes().map((cellType) => cellType.name),
    );
    const created = await this.saves.run(() =>
      this.api.createCellType({
        name,
        kind: 'Text',
        description: null,
        configuration: null,
        style: null,
        options: [],
      }),
    );
    if (!created) {
      return null;
    }
    this.resource.update((cellTypes) => [...(cellTypes ?? []), created]);
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
      configuration: cellType.configuration,
      style: cellType.style,
      options: cellType.options.map((option) => option.value),
      ...changes,
    };
    const updated = await this.saves.run(() => this.api.updateCellType(id, request));
    if (updated) {
      this.resource.update((cellTypes) =>
        (cellTypes ?? []).map((candidate) => (candidate.id === id ? updated : candidate)),
      );
      if (updated.kind !== cellType.kind) {
        // A kind change drops the configuration overrides of the cells that use the type.
        this.clearedOverrides.update((count) => count + 1);
      }
    }
  }

  public async deleteCellType(id: number): Promise<boolean> {
    const deleted = await this.saves.run(async () => {
      await this.api.deleteCellType(id);
      return true;
    });
    if (deleted) {
      this.resource.update((cellTypes) =>
        (cellTypes ?? []).filter((cellType) => cellType.id !== id),
      );
    }
    return deleted === true;
  }
}
