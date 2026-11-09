import { Component, effect, inject, untracked } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ApprovalsStore } from '../../../features/approvals/approvals.store';

/**
 * Tells the user, once when the app opens, that changes are waiting for their approval, with a button to
 * the Approvals screen, and fades after a few seconds. Not on a sheet, whose top line already says what
 * on it waits for them. After that the count on the Approvals entry in the side nav keeps them told.
 */
@Component({
  selector: 'app-approvals-notice',
  imports: [ButtonModule, ToastModule],
  providers: [MessageService],
  templateUrl: './approvals-notice.html',
  styleUrl: './approvals-notice.scss',
})

export class ApprovalsNotice {
  public static readonly KEY = 'approvals';

  public readonly key = ApprovalsNotice.KEY;

  private readonly approvals = inject(ApprovalsStore);
  private readonly messages = inject(MessageService);
  private readonly router = inject(Router);
  private shown = false;

  constructor() {
    effect(() => {
      const count = this.approvals.awaitingCount();
      if (count === 0) {
        // Everything was decided, so the notice no longer holds.
        untracked(() => this.messages.clear(ApprovalsNotice.KEY));
      } else if (!this.shown && !this.onSheet()) {
        this.shown = true;
        untracked(() => this.show(count));
      }
    });
  }

  public open(): void {
    this.messages.clear(ApprovalsNotice.KEY);
    void this.router.navigate(['/approvals']);
  }

  private onSheet(): boolean {
    return /\/sheets\/\d+/.test(this.router.url);
  }

  private show(count: number): void {
    this.messages.add({
      key: ApprovalsNotice.KEY,
      severity: 'secondary',
      summary: 'Waiting for your approval',
      detail: count === 1 ? '1 set of changes is waiting for you.' : `${count} sets of changes are waiting for you.`,
      life: 8000,
    });
  }
}
