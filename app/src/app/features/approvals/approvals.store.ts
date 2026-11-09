import { httpResource } from '@angular/common/http';
import { computed, inject, Service, signal } from '@angular/core';
import { ApiErrorMessage } from '../templates/api-error-message';
import { ApprovalsApi } from './approvals-api';
import { Approver } from './models/approver.model';
import { SheetSubmission } from './models/sheet-submission.model';

/**
 * Publishing with approval, for the whole app: what waits for the user's decision (read as soon as the
 * app opens, so the side nav can say so), what the user submitted, and who they can ask to approve.
 * A decision reloads both lists; the sheet itself hears about it over its live connection.
 */
@Service()

export class ApprovalsStore {
  /** Submissions waiting for the user's decision, oldest first. */
  public readonly awaitingMe = computed<SheetSubmission[]>(() =>
    this.awaitingResource.hasValue() ? this.awaitingResource.value() : [],
  );

  public readonly awaitingCount = computed(() => this.awaitingMe().length);

  /** The user's own submissions, pending or decided in the last 30 days, newest first. */
  public readonly mine = computed<SheetSubmission[]>(() =>
    this.mineResource.hasValue() ? this.mineResource.value() : [],
  );

  /** The people the user can ask to approve; empty until `loadApprovers` is first called. */
  public readonly approvers = computed<Approver[]>(() =>
    this.approversResource.hasValue() ? this.approversResource.value() : [],
  );

  public readonly isLoadingApprovers = computed(() => this.approversResource.isLoading());

  public readonly isLoading = computed(() => this.awaitingResource.isLoading() || this.mineResource.isLoading());

  /** The submission a decision is being sent for, so its buttons can wait. */
  public readonly busyId = signal<number | null>(null);

  /** The reason the last decision failed, until another is made. */
  public readonly error = signal<string | null>(null);

  private readonly api = inject(ApprovalsApi);
  private readonly approversWanted = signal(false);
  private readonly awaitingResource = httpResource<SheetSubmission[]>(() => '/api/submissions/awaiting-me');
  private readonly mineResource = httpResource<SheetSubmission[]>(() => '/api/submissions/mine');
  private readonly approversResource = httpResource<Approver[]>(() =>
    this.approversWanted() ? '/api/approvers' : undefined,
  );

  /** Reads the possible approvers, once; the publish form asks for them when it first opens. */
  public loadApprovers(): void {
    this.approversWanted.set(true);
  }

  public reload(): void {
    this.awaitingResource.reload();
    this.mineResource.reload();
  }

  public approve(submissionId: number): Promise<SheetSubmission | null> {
    return this.decide(submissionId, () => this.api.approve(submissionId));
  }

  public reject(submissionId: number, reason: string): Promise<SheetSubmission | null> {
    return this.decide(submissionId, () => this.api.reject(submissionId, reason));
  }

  public withdraw(submissionId: number): Promise<SheetSubmission | null> {
    return this.decide(submissionId, () => this.api.withdraw(submissionId));
  }

  private async decide(
    submissionId: number,
    call: () => Promise<SheetSubmission>,
  ): Promise<SheetSubmission | null> {
    this.busyId.set(submissionId);
    this.error.set(null);
    try {
      return await call();
    } catch (error) {
      this.error.set(ApiErrorMessage.apiErrorMessage(error));
      return null;
    } finally {
      this.busyId.set(null);
      this.reload();
    }
  }
}
