import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { DatePipe } from '@angular/common';
import {
  ApplicationRef,
  ChangeDetectorRef,
  Component,
  Inject,
  OnInit,
  signal,
} from '@angular/core';
import { DateTime } from 'luxon';
import { ButtonDirective } from 'primeng/button';
import { concat, first, map, takeUntil, takeWhile, timer } from 'rxjs';
import { CheckoutRequest } from '../../../core/models/check-in-out/checkout-request';
import { Destroyable } from '../destructable/destructable';
import { DialogHeader } from '../dialog-header/dialog-header';

@Component({
  selector: 'app-checkout-requested-dialog',
  imports: [DatePipe, DialogHeader, ButtonDirective],
  templateUrl: './checkout-requested-dialog.html',
  styleUrl: './checkout-requested-dialog.scss',
  host: { class: 'dialog card' },
})
export class CheckoutRequestedDialog extends Destroyable implements OnInit {
  public readonly remainingTime = signal<number>(0);

  private _countdownStartDateTime!: DateTime;

  constructor(
    @Inject(DIALOG_DATA)
    private readonly _dialogData: CheckoutRequest,
    private readonly _dialogRef: DialogRef<
      boolean | undefined,
      CheckoutRequestedDialog
    >,
    private readonly _applicationRef: ApplicationRef,
    private readonly _cdr: ChangeDetectorRef,
  ) {
    super();

    this._dialogRef.disableClose = true;
  }

  ngOnInit(): void {
    const appIsStable$ = this._applicationRef.isStable.pipe(
      first((isStable) => isStable === true),
    );

    // Create a 5 minute countdown observable that fires every second
    const countdown$ = timer(0, 1000).pipe(
      map((i) => (300 - i) * 1000),
      takeWhile((n) => n >= 0),
    );

    concat(appIsStable$, countdown$)
      .pipe(
        takeUntil(this.destroy),
        map((n) => {
          if (n === 0) {
            return 0;
          }

          if (!this._countdownStartDateTime) {
            this._countdownStartDateTime = DateTime.now();
          }
          // Find the duration in seconds between the start time and now
          const duration = DateTime.now().diff(
            this._countdownStartDateTime,
            'milliseconds',
          );

          return 300 * 1000 - duration.valueOf();
        }),
      )
      .subscribe((n) => {
        this.remainingTime.set(n);
        this._cdr.detectChanges();

        if (this.remainingTime() === 0) {
          this.reject();
        }
      });
  }

  public accept(): void {
    this._dialogRef.close(true);
  }

  public reject(): void {
    this._dialogRef.close(false);
  }

  public get user(): string {
    return this._dialogData.requestingUser.username;
  }

  public get contextId(): string {
    return this._dialogData.requestContextId;
  }
}
