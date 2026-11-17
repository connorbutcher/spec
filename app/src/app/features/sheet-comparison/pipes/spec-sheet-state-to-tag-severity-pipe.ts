import { Pipe, PipeTransform } from '@angular/core';
import { SpecSheetState } from '../models/spec-sheet-state';

@Pipe({
  name: 'specSheetStateToTagSeverity',
})
export class SpecSheetStateToTagSeverityPipe implements PipeTransform {
  transform(
    value: SpecSheetState,
    ...args: unknown[]
  ):
    | 'success'
    | 'secondary'
    | 'info'
    | 'warn'
    | 'danger'
    | 'contrast'
    | undefined
    | null {
    switch (value) {
      case 'added':
        return 'success';
      case 'removed':
        return 'danger';
      case 'modified':
        return 'warn';
      case 'unchanged':
      default:
        return 'info';
    }
  }
}
