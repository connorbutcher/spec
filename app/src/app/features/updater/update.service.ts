import { Dialog, DialogRef } from '@angular/cdk/dialog';
import { ApplicationRef, Injectable } from '@angular/core';
import { SwUpdate, VersionReadyEvent } from '@angular/service-worker';
import { MessageService } from 'primeng/api';
import {
  BehaviorSubject,
  Observable,
  concat,
  firstValueFrom,
  interval,
} from 'rxjs';
import { filter, first, map } from 'rxjs/operators';
import { NgSwAppData } from './update-available-dialog/data/ngsw-app-data';
import { UpdateDialogResult } from './update-available-dialog/results/update-dialog-result';
import { UpdateAvailableDialog } from './update-available-dialog/update-available-dialog';

@Injectable()
export class UpdateService {
  private readonly _enabled: boolean = true;
  private readonly _deferredInstall = new BehaviorSubject<
    NgSwAppData | undefined
  >(undefined);

  private _updateDialogRef:
    | DialogRef<UpdateDialogResult, UpdateAvailableDialog>
    | undefined;

  constructor(
    private readonly _appRef: ApplicationRef,
    private readonly _swUpdate: SwUpdate,
    private readonly _dialog: Dialog,
    private readonly _toastService: MessageService,
  ) {
    if (!this._swUpdate.isEnabled) {
      console.warn(
        'ServiceWorker has not been enabled or is not supported by this browser.',
      );
      this._enabled = false;
      return;
    }

    // An update is available
    this._swUpdate.versionUpdates
      .pipe(
        filter((evt): evt is VersionReadyEvent => evt.type === 'VERSION_READY'),
        map((evt) => evt.latestVersion),
      )
      .subscribe((updateAvailableEvent) => {
        const availableAppData = updateAvailableEvent.appData as NgSwAppData;

        this.showUpdateAvailableDialog(availableAppData).then(
          async (result) => {
            if (result) {
              switch (result.action) {
                case 'update':
                  await this.activateUpdateAsync();
                  break;
                case 'defer':
                  this._deferredInstall.next(availableAppData);
                  break;
                case 'none':
                  this._deferredInstall.next(undefined);
                  break;
              }
            }
          },
        );
      });

    // There was an error that we can't recover from
    this._swUpdate.unrecoverable.subscribe((event) => {
      this._toastService.add({
        severity: 'error',
        summary: 'Service Worker Error',
        detail: 'An unrecoverable error occurred. Please reload the page.',
      });
    });
  }

  public startUpdateCheckCycle() {
    if (!this._enabled) {
      console.warn(
        'Cannot start service worker check cycle - check for previous warnings.',
      );
      return;
    }

    const appIsStable$ = this._appRef.isStable.pipe(
      first((isStable) => isStable === true),
    );
    const everyFiveMinutes$ = interval(5 * 60 * 1000);
    const everyFiveMinutesOnceAppIsStable$ = concat(
      appIsStable$,
      everyFiveMinutes$,
    );

    everyFiveMinutesOnceAppIsStable$.subscribe(() =>
      this._swUpdate.checkForUpdate(),
    );
  }

  public checkForUpdate() {
    if (!this._enabled) {
      console.warn('Cannot check for update - check for previous warnings');
      return;
    }

    this._swUpdate.checkForUpdate();
  }

  public async activateUpdateAsync() {
    await this._swUpdate.activateUpdate();
    location.reload();
  }

  public get enabled(): boolean {
    return this._enabled;
  }

  /**
   * Gets an observable that is emitted when an update is deferred by the user.
   * The value of the observable is the latest update information @see NgSwAppData. Undefined if there is no deferred update.
   */
  public get deferredInstallStatusChange$(): Observable<
    NgSwAppData | undefined
  > {
    return this._deferredInstall.asObservable();
  }

  public get deferredInstallStatus(): NgSwAppData | undefined {
    return this._deferredInstall.value;
  }

  private async showUpdateAvailableDialog(
    newAppData: NgSwAppData,
  ): Promise<UpdateDialogResult | undefined> {
    if (this._updateDialogRef) {
      this._updateDialogRef.close({ action: 'none' });
    }

    this._updateDialogRef = this._dialog.open<
      UpdateDialogResult,
      NgSwAppData,
      UpdateAvailableDialog
    >(UpdateAvailableDialog, {
      data: newAppData,
      width: '500px',
      minWidth: '500px',
      maxWidth: '500px',
    });

    const result = await firstValueFrom(this._updateDialogRef?.closed);
    this._updateDialogRef = undefined;
    return result;
  }
}
