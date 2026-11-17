import { Pipe, PipeTransform } from '@angular/core';
import { ValidatorLevel } from '../../../core/types/configuration/validator-level.enum';

@Pipe({
  name: 'validatorLevelToAdvisorySeverity',
})
export class ValidatorLevelToAdvisorySeverityPipe implements PipeTransform {
  transform(
    value: ValidatorLevel,
    ...args: unknown[]
  ):
    | 'success'
    | 'info'
    | 'warn'
    | 'danger'
    | 'help'
    | 'primary'
    | 'secondary'
    | 'contrast'
    | null
    | undefined {
    switch (value) {
      case ValidatorLevel.Info:
        return 'info';
      case ValidatorLevel.Warning:
        return 'warn';
      case ValidatorLevel.Error:
        return 'danger';
      default:
        return undefined;
    }
  }
}
