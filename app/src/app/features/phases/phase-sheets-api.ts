import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateSheetRequest } from '../../core/models/create-sheet-request.model';
import { SheetSummary } from '../../core/models/sheet-summary.model';

/** Write calls for a phase's sheets. The list itself is read through `httpResource`. */
@Service()

export class PhaseSheetsApi {
  private readonly http = inject(HttpClient);

  /** Creates a sheet with its identifier values, which can't change afterwards. Needs the `sheets.edit` permission. */
  public create(phaseId: number, request: CreateSheetRequest): Promise<SheetSummary> {
    return firstValueFrom(this.http.post<SheetSummary>(`/api/phases/${phaseId}/sheets`, request));
  }
}
