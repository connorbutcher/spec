import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { SheetAdapter } from '../../../core/helpers/sheet-adapter';
import { SheetDetail } from '../../../core/models/domain/sheets/sheet';
import { SpecSheet } from '../../../core/models/domain/spec-sheet/spec-sheet';
import { SpecSheetSummary } from '../../../core/models/domain/spec-sheet/spec-sheet-summary';
import { SpecSheetChanges } from '../../../core/models/operations/spec-sheet-changes';
import { PublishSpecSheetRequest } from '../../../core/models/requests/spec-sheets/publish-spec-sheet-request';
import { ReassignWorkflowStepActorsRequest } from '../../../core/models/requests/workflow/reassign-workflow-step-actors-request';
import { UpdateWorkflowStepActorRequest } from '../../../core/models/requests/workflow/update-workflow-step-actor-request';
import { SheetListingService } from '../../../core/services/sheet-listing.service';

@Injectable()
export class SheetDataService {
  private readonly _httpClient = inject(HttpClient);
  private readonly _sheetListingService = inject(SheetListingService);

  public async getSpecSheetByIdAsync(id: number): Promise<SpecSheet> {
    const url = `${environment.apiUrl}/sheets/spec-sheet/${id}/detail`;
    const detail = await lastValueFrom(this._httpClient.get<SheetDetail>(url));
    return SheetAdapter.toSpecSheet(detail.sheet, id, detail.submissions);
  }

  public async getAllSpecSheetsByPhaseAndTypeAsync(
    phaseId: number,
    typeId: number,
  ): Promise<SpecSheetSummary[]> {
    return await this._sheetListingService.getByPhaseAndTypeAsync(phaseId, typeId);
  }

  public async updateSpecSheetAsync(request: SpecSheetChanges): Promise<void> {
    const url = `${environment.apiUrl}/sheets/spec-sheet/${request.specSheetId}/draft`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, { operations: request.operations }),
    );
  }

  public async publishSpecSheetAsync(
    request: PublishSpecSheetRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/spec-sheet/${request.id}/publish`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, {
        changeLog: request.changeLog,
        approverUserIds: request.approvers.map((approver) => approver.id),
      }),
    );
  }

  public async updateApprovalStateAsync(
    request: UpdateWorkflowStepActorRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/spec-sheet/${request.specSheetId}/submission/decision`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, {
        decision: request.state,
        comment: request.comment,
      }),
    );
  }

  public async reassignWorkflowStepActorsAsync(
    request: ReassignWorkflowStepActorsRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/spec-sheet/${request.specSheetId}/submission/approvers`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, {
        addedUserIds: request.createWorkflowStepActorRequests,
        removedUserIds: request.deleteWorkflowStepActorRequests,
      }),
    );
  }
}
