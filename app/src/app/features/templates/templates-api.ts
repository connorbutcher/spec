import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CellType } from './models/cell-type.model';
import { SaveCellTypeRequest } from './models/save-cell-type-request.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateOrientation } from './models/template-orientation';
import { UpdateTemplateCellOverridesRequest } from './models/update-template-cell-overrides-request.model';
import { UpdateTemplateCellRequest } from './models/update-template-cell-request.model';
import { UpdateTemplateSectionRequest } from './models/update-template-section-request.model';

/**
 * Write calls for templates and cell types. Reads go through `httpResource` in the store. Every
 * template change returns the whole updated template.
 */
@Service()
export class TemplatesApi {
  private readonly http = inject(HttpClient);

  public createTemplate(
    sheetTypeId: number,
    name: string,
    orientation: TemplateOrientation,
  ): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>('/api/table-templates', { sheetTypeId, name, orientation }),
    );
  }

  public updateTemplate(
    id: number,
    name: string,
    orientation: TemplateOrientation,
  ): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.put<TableTemplate>(`/api/table-templates/${id}`, { name, orientation }),
    );
  }

  public deleteTemplate(id: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`/api/table-templates/${id}`));
  }

  /** Copies the latest version into a new, editable version. */
  public createVersion(templateId: number): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>(`/api/table-templates/${templateId}/versions`, {}),
    );
  }

  public createSection(
    tableTemplateVersionId: number,
    parentSectionId: number | null,
    name: string,
  ): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>('/api/template-sections', {
        tableTemplateVersionId,
        parentSectionId,
        name,
      }),
    );
  }

  public updateSection(id: number, request: UpdateTemplateSectionRequest): Promise<TableTemplate> {
    return firstValueFrom(this.http.put<TableTemplate>(`/api/template-sections/${id}`, request));
  }

  public moveSection(id: number, displayOrder: number): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>(`/api/template-sections/${id}/move`, { displayOrder }),
    );
  }

  public deleteSection(id: number): Promise<TableTemplate> {
    return firstValueFrom(this.http.delete<TableTemplate>(`/api/template-sections/${id}`));
  }

  /**
   * Adds a row at 1-based `position` (or the end), copying the columns of `copyFromRowId` (or the
   * section's last row).
   */
  public createRow(
    templateSectionId: number,
    position: number | null,
    copyFromRowId: number | null,
  ): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>('/api/template-rows', {
        templateSectionId,
        position,
        copyFromRowId,
      }),
    );
  }

  public moveRow(id: number, displayOrder: number): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>(`/api/template-rows/${id}/move`, { displayOrder }),
    );
  }

  public deleteRow(id: number): Promise<TableTemplate> {
    return firstValueFrom(this.http.delete<TableTemplate>(`/api/template-rows/${id}`));
  }

  /** Adds a cell at `column` (moving the rest right), or after the row's last cell when null. */
  public createCell(templateRowId: number, column: number | null): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>('/api/template-cells', {
        templateRowId,
        cellTypeId: null,
        column,
      }),
    );
  }

  public updateCell(id: number, request: UpdateTemplateCellRequest): Promise<TableTemplate> {
    return firstValueFrom(this.http.put<TableTemplate>(`/api/template-cells/${id}`, request));
  }

  /** Replaces what a cell changes from its cell type's default configuration and style. */
  public updateCellOverrides(
    id: number,
    request: UpdateTemplateCellOverridesRequest,
  ): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.put<TableTemplate>(`/api/template-cells/${id}/overrides`, request),
    );
  }

  public deleteCell(id: number): Promise<TableTemplate> {
    return firstValueFrom(this.http.delete<TableTemplate>(`/api/template-cells/${id}`));
  }

  public createCellType(request: SaveCellTypeRequest): Promise<CellType> {
    return firstValueFrom(this.http.post<CellType>('/api/cell-types', request));
  }

  public updateCellType(id: number, request: SaveCellTypeRequest): Promise<CellType> {
    return firstValueFrom(this.http.put<CellType>(`/api/cell-types/${id}`, request));
  }

  public deleteCellType(id: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`/api/cell-types/${id}`));
  }
}
