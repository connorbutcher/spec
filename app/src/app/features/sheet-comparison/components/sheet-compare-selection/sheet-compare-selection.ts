import {
  Component,
  computed,
  effect,
  inject,
  input,
  Signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { Select } from 'primeng/select';
import { distinctUntilChanged } from 'rxjs';
import { SpecSheetType } from '../../../../core/models/domain/types/spec-sheet-type';
import { SheetSummaryPropertiesDisplay } from '../../../properties/components/display/sheet-summary-properties-display/sheet-summary-properties-display';
import { SheetComparisonService } from '../../services/sheet-comparison.service';

@Component({
  selector: 'app-sheet-compare-selection',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    Select,
    InputGroup,
    InputGroupAddon,
    SheetSummaryPropertiesDisplay,
  ],
  templateUrl: './sheet-compare-selection.html',
  styleUrl: './sheet-compare-selection.scss',
})
export class SheetCompareSelection {
  public readonly comparisonSide = input.required<'left' | 'right'>();
  public readonly options = computed(() => {
    const versions = this._specSheetComparisonService.specSheetVersions();
    if (!versions || versions.length === 0) {
      return [];
    }

    const side = this.comparisonSide();
    const otherVersionNumber =
      side === 'left'
        ? this._specSheetComparisonService.rightVersionNumber()
        : this._specSheetComparisonService.leftVersionNumber();
    if (otherVersionNumber === undefined) {
      return versions;
    }

    const indexOfOtherSpecSheet = versions.findIndex(
      (version) => version.versionNumber === otherVersionNumber,
    );

    return versions.filter(
      (version, index) =>
        version.versionNumber !== otherVersionNumber &&
        (side === 'left'
          ? index > indexOfOtherSpecSheet
          : index < indexOfOtherSpecSheet),
    );
  });
  public readonly sideLabel = computed(() => {
    const comparisonSide = this.comparisonSide();
    const specSheetType = this._specSheetComparisonService.specSheetType();
    if (specSheetType?.tableOrientation === 'horizontal') {
      return comparisonSide === 'left' ? 'Top' : 'Bottom';
    } else {
      return comparisonSide === 'left' ? 'Left' : 'Right';
    }
  });
  public readonly selectedSpecSheetCtrl = new FormControl<number>(0, [
    Validators.required,
  ]);

  private readonly _specSheetComparisonService = inject(SheetComparisonService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  constructor() {
    effect(() => {
      const side = this.comparisonSide();
      const versionNumber =
        side === 'left'
          ? this._specSheetComparisonService.leftVersionNumber()
          : this._specSheetComparisonService.rightVersionNumber();
      this.selectedSpecSheetCtrl.setValue(versionNumber ?? 0);
    });

    this.selectedSpecSheetCtrl.valueChanges
      .pipe(takeUntilDestroyed(), distinctUntilChanged())
      .subscribe((selectedVersionNumber) => {
        if (!selectedVersionNumber) {
          return;
        }

        this._router.navigate([], {
          relativeTo: this._activatedRoute,
          queryParamsHandling: 'merge',
          queryParams:
            this.comparisonSide() === 'left'
              ? { left: selectedVersionNumber }
              : { right: selectedVersionNumber },
        });
      });
  }

  public get specSheetType(): Signal<SpecSheetType | undefined> {
    return this._specSheetComparisonService.specSheetType;
  }
}
