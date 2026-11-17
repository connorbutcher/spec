import { HttpClient, httpResource } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { SpecSheetSummary } from '../../../../core/models/domain/spec-sheet/spec-sheet-summary';
import { SpecSheetPhase } from '../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../core/models/domain/types/spec-sheet-type';

@Injectable()
export class SheetsAuditService {
  public readonly activePhase = signal<SpecSheetPhase | undefined>(undefined);
  public readonly activeType = signal<SpecSheetType | undefined>(undefined);
  public readonly specSheetsResource = httpResource<SpecSheetSummary[]>(() =>
    this.activePhase() && this.activeType()
      ? {
          url: `${environment.apiUrl}/specsheets/phase/${this.activePhase()?.id}/type/${this.activeType()?.id}`,
        }
      : undefined,
  );

  private readonly _httpClient = inject(HttpClient);

  public async deleteSpecSheetAsync(specSheetId: number): Promise<void> {
    const url = `${environment.apiUrl}/specsheets/${specSheetId}`;
    return await lastValueFrom(this._httpClient.delete<void>(url));
  }
}
