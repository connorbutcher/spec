import { Component, computed, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TextareaModule } from 'primeng/textarea';
import { DialogActions } from '../../../shared/components/dialog-actions/dialog-actions';

/** Asks why changes are rejected: the reason is required, and the submitter sees it. */
@Component({
  selector: 'app-submission-reject-form',
  imports: [DialogActions, FormsModule, TextareaModule],
  templateUrl: './submission-reject-form.html',
  styleUrl: './submission-reject-form.scss',
})

export class SubmissionRejectForm {
  public readonly busy = input(false);

  public readonly rejected = output<string>();

  public readonly cancelled = output();

  public readonly reason = signal('');

  public readonly canReject = computed(() => this.reason().trim() !== '');

  public reject(): void {
    if (this.canReject()) {
      this.rejected.emit(this.reason().trim());
    }
  }

  public cancel(): void {
    this.reason.set('');
    this.cancelled.emit();
  }
}
