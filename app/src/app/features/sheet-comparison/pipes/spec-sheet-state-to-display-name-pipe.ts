import { Pipe, PipeTransform } from '@angular/core';
import { SpecSheetState } from '../models/spec-sheet-state';

@Pipe({
  name: 'specSheetStateToDisplayName',
})
export class SpecSheetStateToDisplayNamePipe implements PipeTransform {
  transform(value: SpecSheetState, ...args: unknown[]): string {
    switch (value) {
      case 'unchanged':
        return 'Unchanged';
      case 'added':
        return 'Added';
      case 'removed':
        return 'Removed';
      case 'modified':
        return 'Modified';
      default:
        return 'Unknown';
    }
  }
}
