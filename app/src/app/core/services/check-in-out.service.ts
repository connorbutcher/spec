import { computed, Injectable, signal } from '@angular/core';
import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  RetryContext,
} from '@microsoft/signalr';
import { MessageService } from 'primeng/api';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CheckoutRequest } from '../models/check-in-out/checkout-request';
import { CheckoutSummary } from '../models/check-in-out/checkout-summary';
import { UserService } from './user.service';

function reconnectPolicy(retryContext: RetryContext): number | null {
  switch (retryContext.previousRetryCount) {
    case 0:
      return 0; // 0 seconds
    case 1:
      return 2000; // 2 seconds
    case 2:
      return 10000; // 10 seconds
  }

  // Retry every 20 seconds for an additional hour (20seconds * 180 = 1 hour)
  if (retryContext.previousRetryCount < 180) {
    return 20000;
  }

  // Retry every minute for an additional 23 hours (60 seconds * 23 * 60 = 1 hour)
  if (retryContext.previousRetryCount < 23 * 60 * 60) {
    return 60000;
  }

  // Just give up...
  return null;
}

@Injectable({
  providedIn: 'root',
})
export class CheckInOutService {
  public readonly isServiceConnected = signal<boolean>(false);
  public readonly checkoutSummary = signal<CheckoutSummary | undefined>(
    undefined,
    { equal: () => false },
  );
  public readonly checkoutRequested$ = new BehaviorSubject<
    CheckoutRequest | undefined
  >(undefined);
  public readonly checkoutRequestAccepted$ = new Subject<CheckoutRequest>();
  public readonly closeCheckInOutRelatedDialog$ = new BehaviorSubject<
    string | undefined
  >(undefined);

  // public readonly puCardLocked$ = new Subject<string>();
  // public readonly puCardSaved$ = new Subject<string>();
  public readonly puSpecSheetCheckouts = computed(
    () => {
      const checkoutSummary = this.checkoutSummary();
      if (!checkoutSummary) {
        return [];
      }

      return checkoutSummary.puSpecSheetCheckouts;
    },
    { equal: () => false },
  );

  private _hubConnection!: HubConnection;
  private _originalConnectionId: string | undefined;

  constructor(
    private readonly _userService: UserService,
    private readonly _messageService: MessageService,
  ) {}

  /**
   * Start the signalR connection.
   */
  public async startConnectionAsync(
    username: string,
    uid: string,
  ): Promise<void> {
    if (this._hubConnection) {
      return;
    }

    this._hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.signalRUrl}/${environment.signalRHubName}`, {
        transport: HttpTransportType.WebSockets,
        withCredentials: true,
        accessTokenFactory() {
          return `${username}:${uid}`;
        },
      })
      .withAutomaticReconnect({ nextRetryDelayInMilliseconds: reconnectPolicy })
      .build();

    this._hubConnection.keepAliveIntervalInMilliseconds = 5;

    try {
      await this._hubConnection.start();

      this._originalConnectionId = this.currentConnectionId;

      this.handleCompleteDisconnect();
      this.handleReconnection();
    } catch (err) {
      return;
    }

    this.isServiceConnected.set(true);
    this.setupSubscriptions();
  }

  /**
   * Disconnect the signalR connection.
   */
  public async stopConnectionAsync(): Promise<void> {
    if (!this._hubConnection) {
      return;
    }

    await this._hubConnection.stop();
    this.isServiceConnected.set(false);
  }

  //#region Check Out

  public async invokeCheckoutAsync(id: number): Promise<void> {
    if (!this.isHubConnected) {
      return;
    }

    try {
      await this._hubConnection.invoke('CheckoutAsync', id);
    } catch (error) {
      console.error('Error invoking CheckOut:', error);
    }
  }

  //#endregion Check Out

  //#region Check In

  public async invokeCheckInAsync(id: number): Promise<void> {
    if (!this.isHubConnected) {
      return;
    }

    try {
      await this._hubConnection.invoke('CheckInAsync', id);
    } catch (error) {
      console.error('Error invoking CheckIn:', error);
    }
  }

  //#endregion Check In

  //#region Request Check Out

  public async invokeRequestCheckoutAsync(
    id: number,
  ): Promise<CheckoutRequest | undefined> {
    if (!this.isHubConnected) {
      return undefined;
    }

    try {
      return await this._hubConnection.invoke<CheckoutRequest>(
        'RequestCheckoutAsync',
        id,
      );
    } catch (err) {
      return Promise.reject(err);
    }
  }

  public async invokeCancelRequestToCheckoutAsync(
    checkoutRequestContextId: string,
  ): Promise<void> {
    if (!this.isHubConnected) {
      return undefined;
    }

    await this._hubConnection.invoke(
      'CancelRequestToCheckoutAsync',
      checkoutRequestContextId,
    );
  }

  public async invokeOverrideCheckoutRequestAsync(
    checkoutRequestContextId: string,
  ): Promise<void> {
    if (!this.isHubConnected) {
      return undefined;
    }

    await this._hubConnection.invoke(
      'OverrideCheckoutRequestAsync',
      checkoutRequestContextId,
    );
  }

  public async invokeRespondToCheckoutRequestAsync(
    checkoutRequestContextId: string,
    accept: boolean,
  ): Promise<void> {
    if (!this.isHubConnected) {
      return undefined;
    }

    try {
      await this._hubConnection.invoke(
        'RespondToCheckoutRequestAsync',
        checkoutRequestContextId,
        accept,
      );
    } catch (err) {
      return Promise.reject(err);
    }
  }

  //#endregion Request Check Out

  // //#region Saved

  // public async invokePUCardSavedAsync(id: number): Promise<void> {
  //   if (!this.isHubConnected) {
  //     return;
  //   }

  //   try {
  //     await this._hubConnection.invoke('PUCardSavedAsync', id);
  //   } catch (err) {
  //     return Promise.reject(err);
  //   }
  // }

  // //#endregion Saved

  // //#region Locked

  // public async invokePUCardLockedAsync(id: number): Promise<void> {
  //   if (!this.isHubConnected) {
  //     return;
  //   }

  //   try {
  //     await this._hubConnection.invoke('PUCardLockedAsync', id);
  //   } catch (err) {
  //     return Promise.reject(err);
  //   }
  // }

  // //#endregion Locked

  //#region Transfer Ownership

  public async invokeTransferOwnershipAsync(
    oldConnectionId: string,
    checkouts: CheckoutSummary,
  ): Promise<void> {
    if (!this.isHubConnected) {
      return;
    }

    try {
      await this._hubConnection.invoke(
        'TransferOwnershipAsync',
        oldConnectionId,
        checkouts,
      );
    } catch (err) {
      return Promise.reject(err);
    }
  }

  //#endregion Transfer Ownership

  public isPUSpecSheetCheckedOut(id: number): boolean {
    const puSpecSheetCheckouts = this.puSpecSheetCheckouts();
    if (!puSpecSheetCheckouts) {
      return false;
    }

    const puSpecSheet = puSpecSheetCheckouts.find((pss) => pss.id === id);
    return !!puSpecSheet;
  }

  public isPUSpecSheetCheckedOutToYou(id: number): boolean {
    const puSpecSheetCheckouts = this.puSpecSheetCheckouts();
    if (!puSpecSheetCheckouts) {
      return false;
    }

    const user = this._userService.user();
    if (!user) {
      return false;
    }

    const puSpecSheet = puSpecSheetCheckouts.find(
      (p) => p.id === id && p.user.connectionId === this.currentConnectionId,
    );
    return !!puSpecSheet;
  }

  public get currentConnectionId(): string {
    return this._hubConnection?.connectionId ?? '';
  }

  //#region Subscriptions

  private setupSubscriptions(): void {
    this.onCheckInOutSummary();
    this.onCheckoutRequested();
    this.onCancelCheckoutRequest();
    this.onCheckoutRequestAccepted();
    this.onCheckoutRequestRejected();
    this.onOverrideCheckoutRequested();
    // this.onTableCheckoutLost();
    // this.onPUCardSaved();
    // this.onPUCardLocked();
  }

  private onCheckInOutSummary(): void {
    if (!this.isHubConnected) {
      return;
    }

    this._hubConnection.on(
      'CheckInOutSummary',
      (checkoutSummary: CheckoutSummary) => {
        this.checkoutSummary.set(checkoutSummary);
        console.log('Check in/out event occurred', checkoutSummary);
      },
    );
  }

  public onCheckoutRequested(): void {
    if (!this.isHubConnected) {
      return;
    }

    this._hubConnection.on(
      'CheckoutRequested',
      async (request: CheckoutRequest) => {
        this.checkoutRequested$.next(request);
      },
    );
  }

  public onCancelCheckoutRequest(): void {
    if (!this.isHubConnected) {
      return;
    }

    this._hubConnection.on('CancelCheckoutRequest', (requestContextId) => {
      this.closeCheckInOutRelatedDialog$.next(requestContextId);
      this._messageService.add({
        severity: 'info',
        summary: 'Checkout request cancelled by requesting user',
      });
    });
  }

  public onCheckoutRequestAccepted(): void {
    if (!this.isHubConnected) {
      return;
    }

    this._hubConnection.on(
      'CheckoutRequestAccepted',
      (request: CheckoutRequest) => {
        this.checkoutRequestAccepted$.next(request);
        this.closeCheckInOutRelatedDialog$.next(request.requestContextId);
        this._messageService.add({
          severity: 'success',
          summary: `Your checkout request was accepted by ${request.requestingUser.username}`,
        });

        this.checkoutRequested$.next(undefined);
      },
    );
  }

  public onCheckoutRequestRejected(): void {
    if (!this.isHubConnected) {
      return;
    }

    this._hubConnection.on(
      'CheckoutRequestRejected',
      (request: CheckoutRequest) => {
        this.closeCheckInOutRelatedDialog$.next(request.requestContextId);
        this._messageService.add({
          severity: 'warn',
          summary: `Your checkout request was rejected by ${request.requestingUser.username}`,
        });

        this.checkoutRequested$.next(undefined);
      },
    );
  }

  public onOverrideCheckoutRequested(): void {
    if (!this.isHubConnected) {
      return;
    }

    this._hubConnection.on(
      'OverrideCheckoutRequested',
      (request: CheckoutRequest) => {
        if (request.requestingUser.connectionId === this.currentConnectionId) {
          return;
        }

        this.closeCheckInOutRelatedDialog$.next(request.requestContextId);
        this._messageService.add({
          severity: 'warn',
          summary: `Your checkout request was overridden by ${request.respondingUser.username}`,
        });

        this.checkoutRequested$.next(undefined);
      },
    );
  }

  // public onPUCardSaved(): void {
  //   if (!this.isHubConnected) {
  //     return;
  //   }

  //   this._hubConnection.on('PUCardSaved', (id: number, username: string) => {
  //     this._messageService.add({
  //       summary: `This ${this._puCardTerminology.PUCardTypeName} Card has been saved by ${username}`,
  //       severity: 'info',
  //     });
  //     this.puCardSaved$.next(username);
  //   });
  // }

  // public onPUCardLocked(): void {
  //   if (!this.isHubConnected) {
  //     return;
  //   }

  //   this._hubConnection.on('PUCardLocked', (id: number, username: string) => {
  //     this._messageService.add({
  //       summary: `This ${this._puCardTerminology.PUCardTypeName} Card has been locked by ${username}`,
  //       severity: 'info',
  //     });
  //     this.puCardLocked$.next(username);
  //   });
  // }

  // //#endregion Subscriptions

  // //#region Disconnection Handling

  private handleCompleteDisconnect() {
    this._hubConnection.onclose(async () => {
      setTimeout(async () => {
        try {
          const checkoutsForUser = this.getCheckoutsForUser();
          if (!checkoutsForUser) {
            await this._hubConnection.start();
            this._originalConnectionId = this.currentConnectionId;
            return;
          }

          await this._hubConnection.start();
          this._originalConnectionId = this.currentConnectionId;

          await this.invokeTransferOwnershipAsync(
            this._originalConnectionId,
            checkoutsForUser,
          );

          this.setupSubscriptions();
        } catch (err) {}
      }, 5000);
    });
  }

  private handleReconnection() {
    this._hubConnection.onreconnected(() => {
      if (this._originalConnectionId !== this.currentConnectionId) {
        const checkoutsForUser = this.getCheckoutsForUser();
        if (!checkoutsForUser) {
          return;
        }

        try {
          this.invokeTransferOwnershipAsync(
            this._originalConnectionId!,
            checkoutsForUser,
          );

          this._originalConnectionId = this.currentConnectionId;

          this.setupSubscriptions();
        } catch (err) {}
      }
    });
  }

  // //#endregion Disconnection Handling

  private get isHubConnected(): boolean {
    return (
      this._hubConnection &&
      this._hubConnection.state === HubConnectionState.Connected
    );
  }

  private getCheckoutsForUser(): CheckoutSummary | undefined {
    const puSpecSheetCheckouts = this.puSpecSheetCheckouts();

    if (!puSpecSheetCheckouts || puSpecSheetCheckouts.length === 0) {
      return undefined;
    }

    return {
      puSpecSheetCheckouts: puSpecSheetCheckouts.filter(
        (td) => td.user.connectionId === this._originalConnectionId,
      ),
    };
  }
}
