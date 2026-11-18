import { HttpClient } from '@angular/common/http';
import { inject, Injectable, resource, signal } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { SpecSheetPhase } from '../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../core/models/domain/types/spec-sheet-type';
import { SheetListingService } from '../../../../core/services/sheet-listing.service';

@Injectable()
export class SheetsAuditService {
  public readonly activePhase = signal<SpecSheetPhase | undefined>(undefined);
  public readonly activeType = signal<SpecSheetType | undefined>(undefined);
  public readonly specSheetsResource = resource({
    params: () => {
      const phase = this.activePhase();
      const type = this.activeType();
      return phase && type ? { phaseId: phase.id, typeId: type.id } : undefined;
    },
    loader: ({ params }) =>
      this._sheetListingService.getByPhaseAndTypeAsync(
        params.phaseId,
        params.typeId,
      ),
  });

  private readonly _httpClient = inject(HttpClient);
  private readonly _sheetListingService = inject(SheetListingService);

  /** Deletes the draft of the sheet. */
  public async deleteDraftAsync(sheetId: number): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${sheetId}/draft`;
    return await lastValueFrom(this._httpClient.delete<void>(url));
  }
}
