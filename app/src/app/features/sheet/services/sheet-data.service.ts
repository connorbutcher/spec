import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { SpecSheetChanges } from '../../../core/models/operations/spec-sheet-changes';
import { PublishSpecSheetRequest } from '../../../core/models/requests/spec-sheets/publish-spec-sheet-request';
import { DecideSubmissionRequest } from '../../../core/models/requests/sheets/decide-submission-request';
import { ReassignSubmissionApproversRequest } from '../../../core/models/requests/sheets/reassign-submission-approvers-request';
import { SheetListingService } from '../../../core/services/sheet-listing.service';
import { SheetDetail, SheetListItem } from '../../../core/models/domain/sheets/sheet';

@Injectable()
export class SheetDataService {
  private readonly _httpClient = inject(HttpClient);
  private readonly _sheetListingService = inject(SheetListingService);

  /** A published version of the sheet (the latest when no version number is given). */
  public async getPublishedSheetAsync(
    sheetId: number,
    versionNumber?: number,
  ): Promise<SheetDetail> {
    const url = `${environment.apiUrl}/sheets/${sheetId}/detail`;
    return await lastValueFrom(
      this._httpClient.get<SheetDetail>(url, {
        params: versionNumber === undefined ? {} : { version: versionNumber },
      }),
    );
  }

  /** The draft of the sheet. */
  public async getDraftSheetAsync(sheetId: number): Promise<SheetDetail> {
    const url = `${environment.apiUrl}/sheets/${sheetId}/draft/detail`;
    return await lastValueFrom(this._httpClient.get<SheetDetail>(url));
  }

  /** The draft when the list item is the draft, otherwise its published version. */
  public async getSheetAsync(item: SheetListItem): Promise<SheetDetail> {
    return item.isDraft
      ? await this.getDraftSheetAsync(item.sheetId)
      : await this.getPublishedSheetAsync(item.sheetId, item.versionNumber);
  }

  public async getAllSheetsByPhaseAndTypeAsync(
    phaseId: number,
    typeId: number,
  ): Promise<SheetListItem[]> {
    return await this._sheetListingService.getByPhaseAndTypeAsync(phaseId, typeId);
  }

  public async updateSheetAsync(request: SpecSheetChanges): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${request.sheetId}/draft`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, { operations: request.operations }),
    );
  }

  public async publishSheetAsync(request: PublishSpecSheetRequest): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${request.id}/publish`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, {
        changeLog: request.changeLog,
        approverUserIds: request.approvers.map((approver) => approver.id),
      }),
    );
  }

  public async decideSubmissionAsync(
    request: DecideSubmissionRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${request.sheetId}/submission/decision`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, {
        decision: request.decision,
        comment: request.comment,
      }),
    );
  }

  public async reassignSubmissionApproversAsync(
    request: ReassignSubmissionApproversRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${request.sheetId}/submission/approvers`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, {
        addedUserIds: request.addedUserIds,
        removedUserIds: request.removedUserIds,
      }),
    );
  }
}
