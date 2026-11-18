import { Dialog } from '@angular/cdk/dialog';
import { inject, Injectable, Injector } from '@angular/core';
import { MessageService } from 'primeng/api';
import { firstValueFrom } from 'rxjs';
import { SheetSubmission } from '../../../core/models/domain/sheets/sheet-submission';
import { DecideSubmissionRequest } from '../../../core/models/requests/sheets/decide-submission-request';
import { SpinnerDialogService } from '../../../core/services/spinner-dialog.service';
import { ApproverDecision } from '../../../core/types/sheets/approver-decision.enum';
import { RejectSpecSheetDialog } from '../dialogs/reject-spec-sheet-dialog/reject-spec-sheet-dialog';
import { ActiveSheetService } from './active-sheet.service';
import { SheetCheckoutService } from './sheet-checkout.service';
import { SheetDataService } from './sheet-data.service';

/**
 * Deciding and withdrawing the submissions on the open sheet. Each submission is one user's checked-out changes:
 * approving it publishes only those as the next version, rejecting or withdrawing it hands them back to that user.
 */
@Injectable()
export class SheetSubmissionService {
  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sheetDataService = inject(SheetDataService);
  private readonly _sheetCheckoutService = inject(SheetCheckoutService);
  private readonly _spinnerService = inject(SpinnerDialogService);
  private readonly _messageService = inject(MessageService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);

  /** Approves or rejects the submission as the current user; a rejection asks why. */
  public async decideAsync(
    submission: SheetSubmission,
    approve: boolean,
  ): Promise<void> {
    let comment: string | undefined = undefined;
    if (!approve) {
      const rejectReasonDialogRef = this._dialog.open<
        string | undefined,
        unknown,
        RejectSpecSheetDialog
      >(RejectSpecSheetDialog, { injector: this._injector, minWidth: '50%' });
      comment = await firstValueFrom(rejectReasonDialogRef.closed);
      if (!comment) {
        return;
      }
    }

    const ref = this._spinnerService.open(
      'Updating approval state',
      'This should not take long',
    );

    try {
      const request: DecideSubmissionRequest = {
        sheetId: this._activeSheetService.specSheet().id,
        submissionId: submission.id,
        decision: approve ? ApproverDecision.Approved : ApproverDecision.Rejected,
        comment,
      };
      await this._sheetDataService.decideSubmissionAsync(request);

      this._activeSheetService.triggerReload();
      this._messageService.add({
        severity: 'success',
        summary: `${approve ? 'Approved' : 'Rejected'} ${SheetSubmissionService.whoseChanges(submission)}`,
      });
      await this._sheetCheckoutService.notifyAsync(
        approve ? 'approved changes' : 'rejected changes',
      );
    } catch (err) {
      this._messageService.add({
        severity: 'error',
        summary: `Error ${approve ? 'approving' : 'rejecting'} the changes`,
        detail: SheetCheckoutService.reasonOf(err),
      });
    } finally {
      ref.close();
    }
  }

  /** Takes back the current user's submission; its changes go back to being checked out to them. */
  public async withdrawAsync(submission: SheetSubmission): Promise<void> {
    try {
      await this._sheetDataService.withdrawSubmissionAsync(
        this._activeSheetService.specSheet().id,
        submission.id,
      );

      this._activeSheetService.triggerReload();
      this._messageService.add({
        severity: 'success',
        summary: 'Your changes were withdrawn and are checked out to you again',
      });
      await this._sheetCheckoutService.notifyAsync('withdrew changes');
    } catch (err) {
      this._messageService.add({
        severity: 'error',
        summary: 'Error withdrawing the changes',
        detail: SheetCheckoutService.reasonOf(err),
      });
    }
  }

  /** "Jo Bloggs's changes", for messages and headings. */
  public static whoseChanges(submission: SheetSubmission): string {
    const name = submission.submittedByUser?.displayName;
    return name ? `${name}'s changes` : 'the changes';
  }
}
