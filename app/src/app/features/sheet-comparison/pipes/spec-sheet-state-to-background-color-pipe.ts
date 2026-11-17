import { Pipe, PipeTransform } from '@angular/core';
import { SpecSheetState } from '../models/spec-sheet-state';

@Pipe({
  name: 'specSheetStateToBackgroundColor',
})
export class SpecSheetStateToBackgroundColorPipe implements PipeTransform {
  transform(value: SpecSheetState, ...args: unknown[]): string {
    switch (value) {
      case 'added':
        return 'var(--theme-comparison-new-background-color)';
      case 'removed':
        return 'var(--theme-comparison-removed-background-color)';
      case 'modified':
        return 'var(--theme-comparison-modified-background-color)';
      case 'unchanged':
      default:
        return 'transparent';
    }
  }
}
