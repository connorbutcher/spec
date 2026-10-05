import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Phase } from '../../core/models/phase.model';
import { SheetType } from '../../core/models/sheet-type.model';
import { CreatePhaseRequest } from './models/create-phase-request.model';

/** Write calls for the admin section. Reads go through `httpResource` in the stores. */
@Service()
export class AdminApi {
  private readonly http = inject(HttpClient);

  /** Adds a phase after its siblings. Needs the `phases.manage` permission. */
  public createPhase(request: CreatePhaseRequest): Promise<Phase> {
    return firstValueFrom(this.http.post<Phase>('/api/phases', request));
  }

  /** Replaces the sheet types a phase has. Needs the `phases.manage` permission. */
  public setPhaseSheetTypes(phaseId: number, sheetTypeIds: number[]): Promise<Phase> {
    return firstValueFrom(
      this.http.put<Phase>(`/api/phases/${phaseId}/sheet-types`, { sheetTypeIds }),
    );
  }

  /** Adds a sheet type to the end of the list. Needs the `sheetTypes.manage` permission. */
  public createSheetType(name: string): Promise<SheetType> {
    return firstValueFrom(this.http.post<SheetType>('/api/sheet-types', { name }));
  }
}
