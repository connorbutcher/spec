import { Component, computed, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TreeNode } from 'primeng/api';
import { TreeSelect } from 'primeng/treeselect';
import { combineLatest, filter, switchMap, takeUntil } from 'rxjs';
import { IntPropertyValue } from '../../../../../core/models/domain/spec-sheet/properties/values/int-property-value';
import { SpecSheetType } from '../../../../../core/models/domain/types/spec-sheet-type';
import { PhasesService } from '../../../../../core/services/phases.service';
import { PropertyValueType } from '../../../../../core/types/properties/property-value-type.enum';
import { ElementLabel } from '../../../../common/element-label/element-label';
import { SpecSheetTreeHelpers } from '../../../../sheet/helpers/spec-sheet-tree-helpers';
import { PropertiesInfoService } from '../../../services/properties-info.service';
import { PropertyControlBase } from '../property-control-base';
import { CopyFromPropertyDataService } from './copy-from-property-data.service';

@Component({
  selector: 'app-copy-from-property-control',
  imports: [FormsModule, ReactiveFormsModule, ElementLabel, TreeSelect],
  templateUrl: './copy-from-property-control.html',
  styleUrl: './copy-from-property-control.scss',
  providers: [CopyFromPropertyDataService],
})
export class CopyFromPropertyControlComponent extends PropertyControlBase<
  TreeNode,
  IntPropertyValue
> {
  public readonly nodes = signal<TreeNode[]>([]);
  public readonly specSheetType = computed<SpecSheetType>(() =>
    this._propertiesInfoService.extraPropertyInfo().get('specSheetType'),
  );
  public readonly specSheetPhase = computed(() =>
    this._propertiesInfoService.extraPropertyInfo().get('specSheetPhase'),
  );

  private _hasPresetCopyFrom = false;

  constructor(
    private readonly _propertiesInfoService: PropertiesInfoService,
    private readonly _copyFromPropertyDataService: CopyFromPropertyDataService,
    private readonly _phasesService: PhasesService,
  ) {
    super();

    combineLatest([toObservable(this.control), this.formControl.valueChanges])
      .pipe(takeUntilDestroyed())
      .subscribe(([control, value]) => {
        if (!control) {
          return;
        }

        const valueToSet = value.data?.specSheet?.id ?? value.data ?? undefined;

        control.setValue({
          $type: PropertyValueType.Int,
          value: valueToSet,
        });
      });

    combineLatest([
      toObservable(this.specSheetPhase),
      toObservable(this.specSheetType),
    ])
      .pipe(
        takeUntil(this.destroy),
        filter(
          ([specSheetPhase, specSheetType]) =>
            !!specSheetType && !!specSheetPhase,
        ),
        switchMap(([_, specSheetType]) =>
          this._copyFromPropertyDataService.getAllSpecSheetsByTypeAsync(
            specSheetType.id,
          ),
        ),
      )
      .subscribe((summaries) => {
        const specSheetType = this.specSheetType();
        if (!specSheetType.canCopyFromMigrated) {
          summaries = summaries.filter((s) => !s.hasBeenMigrated);
        }

        const phases = this._phasesService
          .flatPhases()
          .filter((p) => p.types.some((t) => t.id === specSheetType.id));

        const tree = SpecSheetTreeHelpers.buildForCopyFrom(
          specSheetType.structureLevels,
          summaries,
          phases,
          this._phasesService.specSheetPhases(),
        );

        this.nodes.set(tree);

        const copyFromSpecSheetId =
          this._propertiesInfoService.getExtraInfo<number>(
            'copyFromSpecSheetId',
          );
        if (copyFromSpecSheetId !== undefined && !this._hasPresetCopyFrom) {
          const pendingNodes = [...tree];
          while (pendingNodes.length > 0) {
            const node = pendingNodes.shift()!;
            if (
              node.type === 'specSheet' &&
              node.data === copyFromSpecSheetId
            ) {
              this.formControl.setValue(node);
              this._hasPresetCopyFrom = true;
              break;
            }
            pendingNodes.push(...(node.children ?? []));
          }
        }
      });
  }
}
