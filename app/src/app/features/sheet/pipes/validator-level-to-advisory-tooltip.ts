import { Pipe, PipeTransform } from '@angular/core';
import { ValidatorLevel } from '../../../core/types/configuration/validator-level.enum';

@Pipe({
  name: 'validatorLevelToAdvisoryTooltip',
})
export class ValidatorLevelToAdvisoryTooltipPipe implements PipeTransform {
  transform(value: ValidatorLevel, ...args: unknown[]): string {
    switch (value) {
      case ValidatorLevel.Info:
        return 'Info';
      case ValidatorLevel.Warning:
        return 'Warning';
      case ValidatorLevel.Error:
        return 'Error';
      default:
        return 'Unknown';
    }
  }
}
