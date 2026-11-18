import { Pipe, PipeTransform } from '@angular/core';
import { SubmissionStatus } from '../../../../../core/types/sheets/submission-status.enum';

@Pipe({
  name: 'approvalStatus',
})
export class ApprovalStatusPipe implements PipeTransform {
  transform(value: SubmissionStatus, ...args: unknown[]): string {
    switch (value) {
      case SubmissionStatus.Approved:
        return 'Approved';
      case SubmissionStatus.Rejected:
        return 'Rejected';
      case SubmissionStatus.Withdrawn:
        return 'Withdrawn';
      case SubmissionStatus.Pending:
        return 'Awaiting Action';
      default:
        return 'Unknown';
    }
  }
}
