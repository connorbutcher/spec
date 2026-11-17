import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { DatePipe } from '@angular/common';
import { Component, Inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DateTime } from 'luxon';
import { ButtonDirective } from 'primeng/button';
import { map, takeUntil, takeWhile, timer } from 'rxjs';
import { CheckoutRequest } from '../../../core/models/check-in-out/checkout-request';
import { CheckInOutService } from '../../../core/services/check-in-out.service';
import { Destroyable } from '../destructable/destructable';
import { DialogHeader } from '../dialog-header/dialog-header';

@Component({
  selector: 'app-requesting-checkout-dialog',
  imports: [DialogHeader, DatePipe, ButtonDirective],
  templateUrl: './requesting-checkout-dialog.html',
  styleUrl: './requesting-checkout-dialog.scss',
  host: { class: 'dialog card' },
})
export class RequestingCheckoutDialog extends Destroyable implements OnInit {
  public readonly remainingTime = signal<number>(0);

  private _countdownStartDateTime!: DateTime;

  constructor(
    @Inject(DIALOG_DATA)
    private readonly _dialogData: CheckoutRequest,
    private readonly _dialogRef: DialogRef<boolean, RequestingCheckoutDialog>,
    private readonly _checkInOutService: CheckInOutService,
  ) {
    super();

    this._dialogRef.disableClose = true;

    this._checkInOutService.checkoutRequestAccepted$
      .pipe(takeUntilDestroyed())
      .subscribe((request) => {
        if (request.id === this._dialogData.id) {
          this._dialogRef.close(true);
        }
      });
  }

  ngOnInit(): void {
    // Initialize countdown start time from the dialog data
    this._countdownStartDateTime = DateTime.fromISO(
      this._dialogData.requestedDateTime,
    );

    // Create a 5 minute countdown observable that fires every second
    timer(0, 1000)
      .pipe(
        takeUntil(this.destroy),
        map(() => {
          // Calculate elapsed time since the request was made
          const duration = DateTime.now().diff(
            this._countdownStartDateTime,
            'milliseconds',
          );

          return Math.max(0, 300 * 1000 - duration.valueOf());
        }),
        takeWhile((n) => n >= 0, true),
      )
      .subscribe(async (n) => {
        this.remainingTime.set(n);

        if (n === 0) {
          await this.closeAsync();
        }
      });
  }

  public async forceAsync(): Promise<void> {
    try {
      await this._checkInOutService.invokeOverrideCheckoutRequestAsync(
        this._dialogData.requestContextId,
      );
      this._dialogRef.close(true);
    } catch (err) {
      this._dialogRef.close(false);
    }
  }

  public async closeAsync(): Promise<void> {
    try {
      await this._checkInOutService.invokeCancelRequestToCheckoutAsync(
        this._dialogData.requestContextId,
      );
    } finally {
      this._dialogRef.close(false);
    }
  }

  public get user(): string {
    return this._dialogData.respondingUser.username;
  }

  public get contextId(): string {
    return this._dialogData.requestContextId;
  }
}
