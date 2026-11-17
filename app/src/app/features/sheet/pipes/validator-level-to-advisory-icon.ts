import { Pipe, PipeTransform } from '@angular/core';
import { ValidatorLevel } from '../../../core/types/configuration/validator-level.enum';

@Pipe({
  name: 'validatorLevelToAdvisoryIcon',
})
export class ValidatorLevelToAdvisoryIconPipe implements PipeTransform {
  transform(value: ValidatorLevel, ...args: unknown[]): string {
    switch (value) {
      case ValidatorLevel.Info:
        return 'pi pi-info-circle';
      case ValidatorLevel.Warning:
        return 'pi pi-exclamation-triangle';
      case ValidatorLevel.Error:
        return 'pi pi-times-circle';
      default:
        return 'Unknown';
    }
  }
}
