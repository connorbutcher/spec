import { Pipe, PipeTransform } from '@angular/core';
import { WorkflowState } from '../../../../../core/models/domain/enums/workflow-state';

@Pipe({
  name: 'workflowStatePipe',
})
export class WorkflowStatePipePipe implements PipeTransform {
  transform(value: WorkflowState, ...args: unknown[]): string {
    switch (value) {
      case WorkflowState.Approved:
        return 'Approved';
      case WorkflowState.Rejected:
        return 'Rejected';
      case WorkflowState.Pending:
        return 'Awaiting Action';
      default:
        return 'Unknown';
    }
  }
}
