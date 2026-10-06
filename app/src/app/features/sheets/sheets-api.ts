import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CellValueRequest } from './models/cell-value-request.model';
import { Sheet } from './models/sheet.model';

/**
 * Write calls for sheets. Reads go through `httpResource` in the store. Every change returns the whole
 * sheet as the current user sees it live, which replaces the open copy.
 */
@Service()
export class SheetsApi {
  private readonly http = inject(HttpClient);

  public addTable(sheetId: number, tableTemplateId: number): Promise<Sheet> {
    return firstValueFrom(
      this.http.post<Sheet>(`/api/sheets/${sheetId}/tables`, { tableTemplateId }),
    );
  }

  public publish(sheetId: number, note: string | null): Promise<Sheet> {
    return firstValueFrom(this.http.post<Sheet>(`/api/sheets/${sheetId}/publish`, { note }));
  }

  public setTableTitle(tableId: number, title: string | null): Promise<Sheet> {
    return firstValueFrom(this.http.put<Sheet>(`/api/sheet-tables/${tableId}`, { title }));
  }

  public moveTable(tableId: number, position: number): Promise<Sheet> {
    return firstValueFrom(
      this.http.post<Sheet>(`/api/sheet-tables/${tableId}/move`, { displayOrder: position }),
    );
  }

  public removeTable(tableId: number): Promise<Sheet> {
    return firstValueFrom(this.http.delete<Sheet>(`/api/sheet-tables/${tableId}`));
  }

  public addSection(
    tableId: number,
    templateSectionId: number,
    parentSheetSectionId: number | null,
  ): Promise<Sheet> {
    return firstValueFrom(
      this.http.post<Sheet>(`/api/sheet-tables/${tableId}/sections`, {
        templateSectionId,
        parentSheetSectionId,
      }),
    );
  }

  public moveSection(sectionId: number, position: number): Promise<Sheet> {
    return firstValueFrom(
      this.http.post<Sheet>(`/api/sheet-sections/${sectionId}/move`, { displayOrder: position }),
    );
  }

  public removeSection(sectionId: number): Promise<Sheet> {
    return firstValueFrom(this.http.delete<Sheet>(`/api/sheet-sections/${sectionId}`));
  }

  public addColumnBlock(tableId: number, templateColumnBlockId: number): Promise<Sheet> {
    return firstValueFrom(
      this.http.post<Sheet>(`/api/sheet-tables/${tableId}/column-blocks`, {
        templateColumnBlockId,
      }),
    );
  }

  public moveColumnBlock(columnBlockId: number, position: number): Promise<Sheet> {
    return firstValueFrom(
      this.http.post<Sheet>(`/api/sheet-column-blocks/${columnBlockId}/move`, {
        displayOrder: position,
      }),
    );
  }

  public removeColumnBlock(columnBlockId: number): Promise<Sheet> {
    return firstValueFrom(this.http.delete<Sheet>(`/api/sheet-column-blocks/${columnBlockId}`));
  }

  public saveValues(rowId: number, values: CellValueRequest[]): Promise<Sheet> {
    return firstValueFrom(this.http.put<Sheet>(`/api/sheet-rows/${rowId}/values`, { values }));
  }

  public moveRow(rowId: number, position: number): Promise<Sheet> {
    return firstValueFrom(
      this.http.post<Sheet>(`/api/sheet-rows/${rowId}/move`, { displayOrder: position }),
    );
  }

  public removeRow(rowId: number): Promise<Sheet> {
    return firstValueFrom(this.http.delete<Sheet>(`/api/sheet-rows/${rowId}`));
  }
}
