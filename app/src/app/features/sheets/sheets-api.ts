import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { firstValueFrom, map, Observable } from 'rxjs';
import { CellSettingsRequest } from './models/cell-settings-request.model';
import { CellValueRequest } from './models/cell-value-request.model';
import { PublishScope } from './models/publish-scope';
import { SheetResponse } from './models/sheet-response.model';
import { SheetRowsChangeResponse } from './models/sheet-rows-change-response.model';
import { SheetRowsChange } from './models/sheet-rows-change.model';
import { Sheet } from './models/sheet.model';
import { SheetResponseUtil } from './data/sheet-response.util';

/**
 * Calls for sheets. The open sheet is read through `httpResource` in the store; the reads here are the
 * quiet ones that follow other people's changes. A change to a row's values or cell settings returns that
 * row and what follows from it; every other change returns the whole sheet as the current user sees it
 * live, which replaces the open copy.
 */
@Service()

export class SheetsApi {
  private readonly http = inject(HttpClient);

  /**
   * The whole live sheet. Read here rather than through a resource because it waits its turn in the
   * store's queue of changes: a resource can't be ordered after a save, and its answer could land on top.
   */
  public live(sheetId: number): Promise<Sheet> {
    // eslint-disable-next-line no-restricted-syntax -- queued behind saves, see above.
    return this.sheet(this.http.get<SheetResponse>(`/api/sheets/${sheetId}`));
  }

  /** Some rows of the live sheet, with what follows from them. Queued like `live`, for the same reason. */
  public rows(sheetId: number, rowIds: readonly number[]): Promise<SheetRowsChange> {
    return this.rowsChange(
      // eslint-disable-next-line no-restricted-syntax -- queued behind saves, see `live`.
      this.http.get<SheetRowsChangeResponse>(`/api/sheets/${sheetId}/rows`, {
        params: { ids: rowIds.map(String) },
      }),
    );
  }

  public addTable(sheetId: number, tableTemplateId: number): Promise<Sheet> {
    return this.sheet(
      this.http.post<SheetResponse>(`/api/sheets/${sheetId}/tables`, { tableTemplateId }),
    );
  }

  public publish(sheetId: number, note: string | null, scope: PublishScope): Promise<Sheet> {
    return this.sheet(
      this.http.post<SheetResponse>(`/api/sheets/${sheetId}/publish`, { note, scope }),
    );
  }

  public setTableTitle(tableId: number, title: string | null): Promise<Sheet> {
    return this.sheet(this.http.put<SheetResponse>(`/api/sheet-tables/${tableId}`, { title }));
  }

  public moveTable(tableId: number, position: number): Promise<Sheet> {
    return this.sheet(
      this.http.post<SheetResponse>(`/api/sheet-tables/${tableId}/move`, {
        displayOrder: position,
      }),
    );
  }

  public removeTable(tableId: number): Promise<Sheet> {
    return this.sheet(this.http.delete<SheetResponse>(`/api/sheet-tables/${tableId}`));
  }

  public addSection(
    tableId: number,
    templateSectionId: number,
    parentSheetSectionId: number | null,
  ): Promise<Sheet> {
    return this.sheet(
      this.http.post<SheetResponse>(`/api/sheet-tables/${tableId}/sections`, {
        templateSectionId,
        parentSheetSectionId,
      }),
    );
  }

  public moveSection(sectionId: number, position: number): Promise<Sheet> {
    return this.sheet(
      this.http.post<SheetResponse>(`/api/sheet-sections/${sectionId}/move`, {
        displayOrder: position,
      }),
    );
  }

  public removeSection(sectionId: number): Promise<Sheet> {
    return this.sheet(this.http.delete<SheetResponse>(`/api/sheet-sections/${sectionId}`));
  }

  public addColumnBlock(tableId: number, templateColumnBlockId: number): Promise<Sheet> {
    return this.sheet(
      this.http.post<SheetResponse>(`/api/sheet-tables/${tableId}/column-blocks`, {
        templateColumnBlockId,
      }),
    );
  }

  public moveColumnBlock(columnBlockId: number, position: number): Promise<Sheet> {
    return this.sheet(
      this.http.post<SheetResponse>(`/api/sheet-column-blocks/${columnBlockId}/move`, {
        displayOrder: position,
      }),
    );
  }

  public removeColumnBlock(columnBlockId: number): Promise<Sheet> {
    return this.sheet(this.http.delete<SheetResponse>(`/api/sheet-column-blocks/${columnBlockId}`));
  }

  public saveValues(rowId: number, values: CellValueRequest[]): Promise<SheetRowsChange> {
    return this.rowsChange(
      this.http.put<SheetRowsChangeResponse>(`/api/sheet-rows/${rowId}/values`, { values }),
    );
  }

  /** Sets or clears the settings chosen on the sheet for cells of a row. A cell whose settings change loses its value. */
  public saveCellSettings(rowId: number, settings: CellSettingsRequest[]): Promise<SheetRowsChange> {
    return this.rowsChange(
      this.http.put<SheetRowsChangeResponse>(`/api/sheet-rows/${rowId}/cell-settings`, { settings }),
    );
  }

  public moveRow(rowId: number, position: number): Promise<Sheet> {
    return this.sheet(
      this.http.post<SheetResponse>(`/api/sheet-rows/${rowId}/move`, { displayOrder: position }),
    );
  }

  public removeRow(rowId: number): Promise<Sheet> {
    return this.sheet(this.http.delete<SheetResponse>(`/api/sheet-rows/${rowId}`));
  }

  /** The sheet a write answers with, each cell given its template cell. */
  private sheet(response: Observable<SheetResponse>): Promise<Sheet> {
    return firstValueFrom(response.pipe(map(SheetResponseUtil.withTemplateCells)));
  }

  /** The rows a call answers with, each cell given its template cell. */
  private rowsChange(response: Observable<SheetRowsChangeResponse>): Promise<SheetRowsChange> {
    return firstValueFrom(response.pipe(map(SheetResponseUtil.rowsWithTemplateCells)));
  }
}
