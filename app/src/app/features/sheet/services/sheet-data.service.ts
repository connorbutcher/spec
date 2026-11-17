import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { SpecSheet } from '../../../core/models/domain/spec-sheet/spec-sheet';
import { SpecSheetSummary } from '../../../core/models/domain/spec-sheet/spec-sheet-summary';
import { SpecSheetChanges } from '../../../core/models/operations/spec-sheet-changes';
import { PublishSpecSheetRequest } from '../../../core/models/requests/spec-sheets/publish-spec-sheet-request';
import { ReassignWorkflowStepActorsRequest } from '../../../core/models/requests/workflow/reassign-workflow-step-actors-request';
import { UpdateWorkflowStepActorRequest } from '../../../core/models/requests/workflow/update-workflow-step-actor-request';

@Injectable()
export class SheetDataService {
  private readonly _httpClient = inject(HttpClient);

  public async getSpecSheetByIdAsync(id: number): Promise<SpecSheet> {
    const url = `${environment.apiUrl}/specsheets/${id}`;
    return await lastValueFrom(this._httpClient.get<SpecSheet>(url));
  }

  public async getAllSpecSheetsByPhaseAndTypeAsync(
    phaseId: number,
    typeId: number,
  ): Promise<SpecSheetSummary[]> {
    const url = `${environment.apiUrl}/specsheets/phase/${phaseId}/type/${typeId}`;
    return await lastValueFrom(this._httpClient.get<SpecSheetSummary[]>(url));
  }

  public async updateSpecSheetAsync(request: SpecSheetChanges): Promise<void> {
    const url = `${environment.apiUrl}/specsheets/update`;
    await lastValueFrom(this._httpClient.patch<void>(url, request));
  }

  public async publishSpecSheetAsync(
    request: PublishSpecSheetRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/specsheets/publish`;
    await lastValueFrom(this._httpClient.patch<void>(url, request));
  }

  public async updateApprovalStateAsync(
    request: UpdateWorkflowStepActorRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/workflow/actor`;
    await lastValueFrom(this._httpClient.patch<void>(url, request));
  }

  public async reassignWorkflowStepActorsAsync(
    request: ReassignWorkflowStepActorsRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/workflow/reassign`;
    await lastValueFrom(this._httpClient.patch<void>(url, request));
  }
}
