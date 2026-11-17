import { Component, effect, inject, Signal } from '@angular/core';
import {
  takeUntilDestroyed,
  toObservable,
  toSignal,
} from '@angular/core/rxjs-interop';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TreeNode } from 'primeng/api';
import { Divider } from 'primeng/divider';
import { IftaLabel } from 'primeng/iftalabel';
import { Select } from 'primeng/select';
import { TreeSelect } from 'primeng/treeselect';
import { combineLatest, distinctUntilChanged, filter, map } from 'rxjs';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { SpecSheetPhase } from '../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../core/models/domain/types/spec-sheet-type';
import { PhasesService } from '../../../../core/services/phases.service';
import { TypesService } from '../../../../core/services/types.service';
import { SheetComparisonService } from '../../services/sheet-comparison.service';
import { SpecSheetComparisonToolbar } from '../sheet-comparison-toolbar/sheet-comparison-toolbar';

interface SpecSheetComparisonHeaderForm {
  type: FormControl<SpecSheetType | null>;
  phase: FormControl<TreeNode<SpecSheetPhase> | null>;
}

@Component({
  selector: 'app-sheet-comparison-header',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TreeSelect,
    Select,
    IftaLabel,
    SpecSheetComparisonToolbar,
    Divider,
  ],
  templateUrl: './sheet-comparison-header.html',
  styleUrl: './sheet-comparison-header.scss',
})
export class SheetComparisonHeader {
  public readonly form: FormGroup<SpecSheetComparisonHeaderForm>;
  public readonly phaseOptions: Signal<TreeNode<SpecSheetPhase>[]>;

  private readonly _specSheetComparisonService = inject(SheetComparisonService);
  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);
  private readonly _formBuilder = inject(FormBuilder);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  constructor() {
    this.form = this.buildForm();

    effect(() => {
      const type = this._specSheetComparisonService.specSheetType();
      const phase = this._specSheetComparisonService.specSheetPhase();

      this.typeCtrl.setValue(type ?? null);
      if (phase) {
        const phaseNode = this.getTreeNodeByPhaseId(phase.id);
        this.phaseCtrl.setValue(phaseNode ?? null);
      }
    });

    this.typeCtrl.valueChanges.pipe(takeUntilDestroyed()).subscribe((type) => {
      this.phaseCtrl.setValue(null);
    });

    this.form.valueChanges
      .pipe(
        takeUntilDestroyed(),
        distinctUntilChanged((x, y) => UpdateHelpers.isEqual(x, y)),
      )
      .subscribe((value) => {
        const type = value.type;
        const phase = value.phase?.data ?? null;

        this._router.navigate([], {
          relativeTo: this._activatedRoute,
          queryParams: {
            type: type ? type.id : undefined,
            phase: phase ? phase.id : undefined,
            left: undefined,
            right: undefined,
          },
          queryParamsHandling: 'merge',
        });
      });

    this.phaseOptions = toSignal(
      combineLatest([
        toObservable(this._specSheetComparisonService.specSheetType),
        toObservable(this._phasesService.specSheetPhases),
      ]).pipe(
        takeUntilDestroyed(),
        distinctUntilChanged(),
        filter(([type, phases]) => !!type),
        map(([type, phases]) => this.buildPhaseOptions(type!, phases)),
      ),
      { initialValue: [] },
    );
  }

  public get typeOptions(): Signal<SpecSheetType[]> {
    return this._typesService.specSheetTypes;
  }

  public get specSheetType(): Signal<SpecSheetType | undefined> {
    return this._specSheetComparisonService.specSheetType;
  }

  public get specSheetPhase(): Signal<SpecSheetPhase | undefined> {
    return this._specSheetComparisonService.specSheetPhase;
  }

  private getTreeNodeByPhaseId(
    phaseId: number,
  ): TreeNode<SpecSheetPhase> | undefined {
    const nodes = this.phaseOptions();
    for (const node of nodes) {
      if (node.key === `${phaseId}`) {
        return node;
      }

      if (node.children) {
        for (const child of node.children) {
          if (child.key === `${phaseId}`) {
            return child;
          }
        }
      }
    }

    return undefined;
  }

  private buildForm(): FormGroup<SpecSheetComparisonHeaderForm> {
    return this._formBuilder.group<SpecSheetComparisonHeaderForm>({
      type: new FormControl<SpecSheetType | null>(null, [Validators.required]),
      phase: new FormControl<TreeNode<SpecSheetPhase> | null>(null, [
        Validators.required,
      ]),
    });
  }

  private buildPhaseOptions(
    type: SpecSheetType | null,
    phases: SpecSheetPhase[],
  ): TreeNode<SpecSheetPhase>[] {
    if (!type || phases.length === 0) {
      return [];
    }

    const nodes: TreeNode[] = [];
    for (const phase of phases) {
      const phaseHasType = phase.types.some((t) => t.id === type.id);
      const subPhaseHasType = phase.phases.some((subPhase) =>
        subPhase.types.some((t) => t.id === type.id),
      );

      nodes.push({
        key: `${phase.id}`,
        label: phase.phase,
        data: phase,
        selectable: phaseHasType,
        styleClass: phaseHasType || subPhaseHasType ? '' : 'p-disabled',
        children: phase.phases.map((subPhase) => {
          const subPhaseHasType = subPhase.types.some((t) => t.id === type.id);

          return {
            key: `${subPhase.id}`,
            label: subPhase.phase,
            data: subPhase,
            selectable: subPhaseHasType,
            styleClass: subPhaseHasType ? '' : 'p-disabled',
          };
        }),
      });
    }

    return nodes;
  }

  private get typeCtrl(): FormControl<SpecSheetType | null> {
    return this.form.get('type') as FormControl<SpecSheetType | null>;
  }

  private get phaseCtrl(): FormControl<TreeNode<SpecSheetPhase> | null> {
    return this.form.get(
      'phase',
    ) as FormControl<TreeNode<SpecSheetPhase> | null>;
  }
}
