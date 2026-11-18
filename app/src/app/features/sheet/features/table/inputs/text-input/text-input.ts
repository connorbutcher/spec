import { A11yModule } from '@angular/cdk/a11y';
import {
  Component,
  computed,
  effect,
  inject,
  input,
  model,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { isNil } from 'lodash';
import { FocusDirective } from '../../../../../../core/directives/focus.directive';
import { ActivationService } from '../../../../../../core/services/activation.service';
import { ActiveSheetService } from '../../../../services/active-sheet.service';

@Component({
  selector: 'app-text-input',
  imports: [FormsModule, A11yModule, FocusDirective],
  templateUrl: './text-input.html',
  styleUrl: './text-input.scss',
})
export class TextInput {
  public readonly value = model.required<string | undefined>();
  public readonly referenceId = input.required<string>();
  /** Set when the row or table holding the input is checked out to someone else. */
  public readonly readOnly = input<boolean>(false);
  public readonly canEdit = computed(
    () => this._sheetService.canEdit() && !this.readOnly(),
  );
  public readonly isCellFocused = signal<boolean>(false);

  public readonly hasValue = computed(() => !isNil(this.value()));

  protected readonly _sheetService = inject(ActiveSheetService);
  protected readonly _activationService = inject(ActivationService);

  constructor() {
    effect(() => {
      const refToActivate = this._activationService.refToActivate();
      if (!refToActivate || refToActivate !== this.referenceId()) {
        return;
      }

      this._activationService.refToActivate.set(undefined);
      this.focusCell(new Event('fragment-activated'));
    });
  }

  public focusCell(event: any): void {
    if (!this.canEdit()) {
      return;
    }

    if (!event) {
      this.isCellFocused.set(false);
      return;
    }

    this.isCellFocused.set(true);
  }
}
