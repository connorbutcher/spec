import {
  Component,
  computed,
  effect,
  input,
  OnInit,
  signal,
} from '@angular/core';
import { FormControl } from '@angular/forms';
import { toLower } from 'lodash';
import { takeUntil } from 'rxjs';
import { SpecSheetTypeProperty } from '../../../../core/models/domain/types/spec-sheet-type-property';
import { PropertyValidatorType } from '../../../../core/types/properties/property-validator-type.enum';
import { Destroyable } from '../../../common/destructable/destructable';
import { PropertyValidatorHelpers } from '../../helpers/property-validator-helpers';

@Component({
  selector: 'app-property-control-base',
  template: '',
})
export class PropertyControlBase<TForm, TValue>
  extends Destroyable
  implements OnInit
{
  public readonly property = input.required<SpecSheetTypeProperty>();
  public readonly control = input.required<FormControl<TValue>>();
  public readonly validationMessages = signal<string[]>([]);
  public readonly formControl = new FormControl<TForm>(undefined!, {
    nonNullable: true,
  });
  public readonly isValid = signal(false);

  public readonly isRequired = computed(
    () =>
      this.property().validators?.some(
        (x) => x.validator === PropertyValidatorType.Required,
      ) ?? false,
  );

  constructor() {
    super();

    effect(() => {
      const property = this.property();
      const validators = property.validators ?? [];
      this.isValid.set(validators.length === 0);

      for (const validator of validators) {
        this.formControl.addValidators(
          PropertyValidatorHelpers.getValidator(
            validator.validator,
            validator.property,
          ),
        );
      }
    });
  }

  ngOnInit(): void {
    this.formControl.statusChanges
      .pipe(takeUntil(this.destroy))
      .subscribe((status) => {
        this.validationMessages.set([]);

        if (status === 'VALID') {
          this.isValid.set(true);
          return;
        }

        for (const validator of this.property().validators ?? []) {
          if (this.formControl.hasError(toLower(validator.validator))) {
            this.isValid.set(false);
            this.validationMessages.update((messages) => {
              return [...messages, validator.message ?? ''];
            });
          }
        }
      });
  }
}
