import { HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';
import { CheckInOutService } from '../../../core/services/check-in-out.service';
import { ActiveSheetService } from './active-sheet.service';
import { SheetDataService } from './sheet-data.service';

/**
 * The current user's checkouts on the open sheet: undoing them, and telling the others viewing the sheet when they
 * change so they read it again.
 */
@Injectable()
export class SheetCheckoutService {
  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sheetDataService = inject(SheetDataService);
  private readonly _checkInOutService = inject(CheckInOutService);
  private readonly _messageService = inject(MessageService);

  /** Undoes every change the current user has checked out on the sheet. */
  public async undoMineAsync(): Promise<void> {
    const sheetId = this._activeSheetService.specSheet().id;
    await this.runAsync(
      () => this._sheetDataService.undoMyChangesAsync(sheetId),
      'Your changes were undone',
      'undid their changes',
    );
  }

  /** Undoes the current user's changes to one row. */
  public async undoRowAsync(rowId: number): Promise<void> {
    const sheetId = this._activeSheetService.specSheet().id;
    await this.runAsync(
      () => this._sheetDataService.undoMyRowChangesAsync(sheetId, rowId),
      'Your changes to the row were undone',
      'undid changes to a row',
    );
  }

  /** Undoes the current user's changes to one group and its rows. */
  public async undoGroupAsync(groupId: number): Promise<void> {
    const sheetId = this._activeSheetService.specSheet().id;
    await this.runAsync(
      () => this._sheetDataService.undoMyGroupChangesAsync(sheetId, groupId),
      'Your changes to the row were undone',
      'undid changes to a row',
    );
  }

  /** Undoes the current user's changes to one table and what is inside it. */
  public async undoTableAsync(tableId: number): Promise<void> {
    const sheetId = this._activeSheetService.specSheet().id;
    await this.runAsync(
      () => this._sheetDataService.undoMyTableChangesAsync(sheetId, tableId),
      'Your changes to the table were undone',
      'undid changes to a table',
    );
  }

  /** Tells the others viewing the sheet that the current user changed it, so they read it again. */
  public async notifyAsync(change: string): Promise<void> {
    await this._checkInOutService.notifySheetChangedAsync(
      this._activeSheetService.specSheet().id,
      change,
    );
  }

  /** The reason the API gave for refusing a request, such as a row being checked out to someone else. */
  public static reasonOf(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      return err.error?.detail ?? err.message;
    }

    return (err as Error)?.message ?? String(err);
  }

  private async runAsync(
    action: () => Promise<void>,
    summary: string,
    change: string,
  ): Promise<void> {
    try {
      await action();
      this._messageService.add({ severity: 'success', summary });
      this._activeSheetService.triggerReload();
      await this.notifyAsync(change);
    } catch (err) {
      this._messageService.add({
        severity: 'error',
        summary: 'Could not undo the changes',
        detail: SheetCheckoutService.reasonOf(err),
      });
    }
  }
}
