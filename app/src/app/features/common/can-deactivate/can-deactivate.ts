import { Component } from '@angular/core';
import { DeactivationType } from '../../../core/types/deactivation-type.enum';

@Component({
  selector: 'app-can-deactivate',
  template: '',
  standalone: true,
})
export abstract class CanDeactivate {
  public abstract hasChanges(): boolean;
  public abstract handleExitAsync(
    deactivationType: DeactivationType,
  ): Promise<void>;
}
