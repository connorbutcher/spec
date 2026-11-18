import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { SheetChanges } from '../../../core/models/operations/sheet-changes';
import { PublishSheetRequest } from '../../../core/models/requests/sheets/publish-sheet-request';
import { DecideSubmissionRequest } from '../../../core/models/requests/sheets/decide-submission-request';
import { ReassignSubmissionApproversRequest } from '../../../core/models/requests/sheets/reassign-submission-approvers-request';
import { SheetListingService } from '../../../core/services/sheet-listing.service';
import { SheetDetail } from '../../../core/models/domain/sheets/sheet-detail';
import { SheetListItem } from '../../../core/models/domain/sheets/sheet-list-item';

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

  /** The sheet as it stands now: everyone's checked-out changes over the latest version. */
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
    return await this._sheetListingService.getByPhaseAndTypeAsync(
      phaseId,
      typeId,
    );
  }

  public async updateSheetAsync(request: SheetChanges): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${request.sheetId}/draft`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, { operations: request.operations }),
    );
  }

  public async publishSheetAsync(request: PublishSheetRequest): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${request.id}/publish`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, {
        changeLog: request.changeLog,
        approverUserIds: request.approvers.map((approver) => approver.id),
      }),
    );
  }

  /** Takes back the current user's pending submission; its changes go back to being checked out to them. */
  public async withdrawSubmissionAsync(
    sheetId: number,
    submissionId: number,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${sheetId}/submissions/${submissionId}/withdraw`;
    await lastValueFrom(this._httpClient.patch<void>(url, {}));
  }

  /** Undoes every change the current user has checked out on the sheet. */
  public async undoMyChangesAsync(sheetId: number): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${sheetId}/draft/mine`;
    await lastValueFrom(this._httpClient.delete<void>(url));
  }

  /** Undoes the current user's checked-out changes to one row, and frees it. */
  public async undoMyRowChangesAsync(
    sheetId: number,
    rowId: number,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${sheetId}/draft/mine/rows/${rowId}`;
    await lastValueFrom(this._httpClient.delete<void>(url));
  }

  /** Undoes the current user's checked-out changes to one group and its rows, and frees them. */
  public async undoMyGroupChangesAsync(
    sheetId: number,
    groupId: number,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${sheetId}/draft/mine/groups/${groupId}`;
    await lastValueFrom(this._httpClient.delete<void>(url));
  }

  /** Undoes the current user's checked-out changes to one table and what is inside it, and frees them. */
  public async undoMyTableChangesAsync(
    sheetId: number,
    tableId: number,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${sheetId}/draft/mine/tables/${tableId}`;
    await lastValueFrom(this._httpClient.delete<void>(url));
  }

  public async decideSubmissionAsync(
    request: DecideSubmissionRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/sheets/${request.sheetId}/submissions/${request.submissionId}/decision`;
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
    const url = `${environment.apiUrl}/sheets/${request.sheetId}/submissions/${request.submissionId}/approvers`;
    await lastValueFrom(
      this._httpClient.patch<void>(url, {
        addedUserIds: request.addedUserIds,
        removedUserIds: request.removedUserIds,
      }),
    );
  }
}
