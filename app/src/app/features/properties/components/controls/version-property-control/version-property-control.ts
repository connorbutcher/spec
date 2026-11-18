import { Component, computed, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormsModule,
  ReactiveFormsModule,
  ValidatorFn,
} from '@angular/forms';
import { isNil } from 'lodash';
import { InputNumber } from 'primeng/inputnumber';
import { combineLatest, filter, switchMap, takeUntil } from 'rxjs';
import { IntPropertyValue } from '../../../../../core/models/domain/spec-sheet/properties/values/int-property-value';
import { SpecSheetPhase } from '../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../core/models/domain/types/spec-sheet-type';
import { PropertyControlType } from '../../../../../core/types/properties/property-control-type.enum';
import { PropertyValueType } from '../../../../../core/types/properties/property-value-type.enum';
import { ElementLabel } from '../../../../common/element-label/element-label';
import { SheetPropertyHelpers } from '../../../../../core/helpers/sheet-property-helpers';
import { PropertiesInfoService } from '../../../services/properties-info.service';
import { PropertyControlBase } from '../property-control-base';
import { VersionPropertyDataService } from './version-property-data.service';
import { SheetListItem } from '../../../../../core/models/domain/sheets/sheet';

@Component({
  selector: 'app-version-property-control',
  imports: [FormsModule, ReactiveFormsModule, ElementLabel, InputNumber],
  templateUrl: './version-property-control.html',
  styleUrl: './version-property-control.scss',
  providers: [VersionPropertyDataService],
})
export class VersionPropertyControl extends PropertyControlBase<
  number,
  IntPropertyValue
> {
  public readonly specSheetTypeId = computed<SpecSheetType>(() =>
    this._propertiesInfoService.extraPropertyInfo().get('specSheetType'),
  );
  public readonly specSheetPhaseId = computed<SpecSheetPhase>(() =>
    this._propertiesInfoService.extraPropertyInfo().get('specSheetPhase'),
  );

  private readonly _summaries = signal<SheetListItem[]>([]);

  constructor(
    private readonly _propertiesInfoService: PropertiesInfoService,
    private readonly _versionPropertyDataService: VersionPropertyDataService,
  ) {
    super();

    this.formControl.setValidators(
      this.versionNumberUniqueValidator(() => this._summaries()),
    );

    combineLatest([toObservable(this.control), this.formControl.valueChanges])
      .pipe(takeUntilDestroyed())
      .subscribe(([control, value]) => {
        if (!control) {
          return;
        }

        control.setValue({
          $type: PropertyValueType.Double,
          value: value ?? undefined,
        });
      });

    combineLatest([
      toObservable(this.specSheetPhaseId),
      toObservable(this.specSheetTypeId),
    ])
      .pipe(
        takeUntil(this.destroy),
        filter(
          ([specSheetPhase, specSheetType]) =>
            !!specSheetType && !!specSheetPhase,
        ),
        switchMap(([specSheetPhase, specSheetType]) =>
          this._versionPropertyDataService.getAllSpecSheetsByPhaseAndTypeAsync(
            specSheetPhase.id,
            specSheetType.id,
          ),
        ),
      )
      .subscribe((summaries) => {
        this._summaries.set(summaries);

        // Get the latest version number and add 1 to it for the default value of the version control
        const versionNumbers = summaries
          .map((s) => {
            const versionProperty = s.properties.find(
              (pv) =>
                pv.specSheetTypeProperty?.valueControlType ===
                PropertyControlType.Version,
            );
            if (!versionProperty) {
              return null;
            }

            return SheetPropertyHelpers.getValue<number>(versionProperty);
          })
          .filter((v) => !isNil(v));
        const latestVersionNumber = versionNumbers.length
          ? Math.max(...versionNumbers)
          : 0;
        this.formControl.setValue(latestVersionNumber + 1);
      });
  }

  private versionNumberUniqueValidator(
    getOptions: () => SheetListItem[],
  ): ValidatorFn {
    return (control: AbstractControl) => {
      const value = control.value as number | null | undefined;
      if (value == null) {
        return null;
      }

      const options = getOptions() ?? [];
      const versionNumbers = options
        .map((o) => {
          const versionProperty = o.properties.find(
            (pv) =>
              pv.specSheetTypeProperty?.valueControlType ===
              PropertyControlType.Version,
          );
          if (!versionProperty) {
            return null;
          }

          return SheetPropertyHelpers.getValue<number>(versionProperty);
        })
        .filter((v) => !isNil(v));
      const exists = versionNumbers.some((vn) => vn === value);
      return exists ? { versionExists: true } : null;
    };
  }
}
