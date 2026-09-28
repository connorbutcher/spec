import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CellType } from './models/cell-type.model';
import { SaveCellTypeRequest } from './models/save-cell-type-request.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateOrientation } from './models/template-orientation';
import { UpdateTemplateCellRequest } from './models/update-template-cell-request.model';

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

  public createSection(
    tableTemplateId: number,
    parentSectionId: number | null,
    name: string,
  ): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>('/api/template-sections', {
        tableTemplateId,
        parentSectionId,
        name,
      }),
    );
  }

  public renameSection(id: number, name: string): Promise<TableTemplate> {
    return firstValueFrom(this.http.put<TableTemplate>(`/api/template-sections/${id}`, { name }));
  }

  public moveSection(id: number, displayOrder: number): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>(`/api/template-sections/${id}/move`, { displayOrder }),
    );
  }

  public deleteSection(id: number): Promise<TableTemplate> {
    return firstValueFrom(this.http.delete<TableTemplate>(`/api/template-sections/${id}`));
  }

  public createRow(templateSectionId: number): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>('/api/template-rows', { templateSectionId }),
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

  public createCell(templateRowId: number): Promise<TableTemplate> {
    return firstValueFrom(
      this.http.post<TableTemplate>('/api/template-cells', { templateRowId, cellTypeId: null }),
    );
  }

  public updateCell(id: number, request: UpdateTemplateCellRequest): Promise<TableTemplate> {
    return firstValueFrom(this.http.put<TableTemplate>(`/api/template-cells/${id}`, request));
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
