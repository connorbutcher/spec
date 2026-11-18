import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import {
  inject,
  Injectable,
  Injector,
  runInInjectionContext,
  signal,
} from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { SheetAdapter } from '../../../core/helpers/sheet-adapter';
import { Sheet } from '../../../core/models/domain/sheets/sheet';
import { SpecSheet } from '../../../core/models/domain/spec-sheet/spec-sheet';
import { PUSpecSheet } from '../models/pu-spec-sheet';
import { SheetContext } from './sheet-context';

@Injectable()
export class LinkedSheetService {
  private readonly _specSheets = signal(new Map<string, PUSpecSheet>());
  private readonly _activeSheetService = inject(SheetContext);
  private readonly _httpClient = inject(HttpClient);
  private readonly _injector = inject(Injector);

  private buildKey(typeId: number, phaseId: number): string {
    return `${typeId}-${phaseId}`;
  }

  public setLinkedSpecSheet(specSheet: SpecSheet): PUSpecSheet {
    const key = this.buildKey(
      specSheet.specSheetTypeId,
      specSheet.specSheetPhaseId,
    );
    const existingSpecSheet = this._specSheets().get(key);
    if (existingSpecSheet) {
      return existingSpecSheet;
    }

    const puSpecSheet = runInInjectionContext(
      this._injector,
      () => new PUSpecSheet(specSheet),
    );

    this._specSheets.update((sheets) => new Map(sheets).set(key, puSpecSheet));

    return puSpecSheet;
  }

  public hasLinkedSpecSheet(typeId: number, phaseId: number): boolean {
    return this._specSheets().has(this.buildKey(typeId, phaseId));
  }

  public getLinkedSpecSheet(
    typeId: number,
    phaseId: number,
  ): PUSpecSheet | undefined {
    return this._specSheets().get(this.buildKey(typeId, phaseId));
  }

  public reset(): void {
    this._specSheets.set(new Map<string, PUSpecSheet>());
  }

  public async getLatestPublishedSpecSheetByTypeAndPhaseAsync(
    typeId: number,
    phaseId: number,
    lockToLatestPublished: boolean,
    publishedByDateTime?: string,
  ): Promise<SpecSheet | undefined> {
    const specSheet = this._activeSheetService.specSheet();

    let params = new HttpParams();
    if (lockToLatestPublished) {
      const publishedDateTime =
        publishedByDateTime ?? specSheet.specSheet.publishedDateTime;
      if (publishedDateTime) {
        params = params.append('publishedBy', publishedDateTime);
      }
    }

    const url = `${environment.apiUrl}/sheets/phase/${phaseId}/type/${typeId}/latest`;
    try {
      const sheet = await lastValueFrom(
        this._httpClient.get<Sheet>(url, { params }),
      );
      return SheetAdapter.toSpecSheet(sheet, sheet.version?.sourceSpecSheetId ?? 0);
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 404) {
        return undefined;
      }

      throw error;
    }
  }
}
