import { Dialog } from '@angular/cdk/dialog';
import { Component, DestroyRef, OnInit, Signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { Toast } from 'primeng/toast';
import { filter } from 'rxjs';
import { User } from './core/models/domain/access/user';
import { BrowserCommunicationProcessorService } from './core/services/browser-communication-processor.service';
import { CheckInOutService } from './core/services/check-in-out.service';
import { UserService } from './core/services/user.service';
import { PUCardAnalyticsBanner } from './features/analytics/components/pu-card-analytics-banner/pu-card-analytics-banner';
import { CheckoutRequestedDialog } from './features/common/checkout-requested-dialog/checkout-requested-dialog';
import { RequestingCheckoutDialog } from './features/common/requesting-checkout-dialog/requesting-checkout-dialog';
import { SideNav } from './features/side-nav/side-nav';
import { UpdateService } from './features/updater/update.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SideNav, Toast, PUCardAnalyticsBanner],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  constructor(
    private readonly _userService: UserService,
    private readonly _updateService: UpdateService,
    private readonly _checkInOutService: CheckInOutService,
    private readonly _dialog: Dialog,
    private readonly _destroyRef: DestroyRef,
    private readonly _browserCommunicationProcessorService: BrowserCommunicationProcessorService,
  ) {
    this._checkInOutService.closeCheckInOutRelatedDialog$
      .pipe(
        takeUntilDestroyed(this._destroyRef),
        filter((id) => !!id),
      )
      .subscribe((requestContextId) => {
        const openDialogs = this._dialog.openDialogs;
        // Find the open dialog with the matching context id
        const dialog = openDialogs.find(
          (d) =>
            (
              d.componentInstance as
                | CheckoutRequestedDialog
                | RequestingCheckoutDialog
            ).contextId === requestContextId,
        );
        if (dialog) {
          if (dialog.componentInstance instanceof CheckoutRequestedDialog) {
            dialog.close(undefined);
          } else {
            dialog.close(false);
          }
        }
      });
  }

  ngOnInit(): void {
    this._updateService.startUpdateCheckCycle();
  }

  public get user(): Signal<User | undefined> {
    return this._userService.user;
  }
}
