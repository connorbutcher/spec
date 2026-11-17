import { Component, computed, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  FormsModule,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { TreeNode } from 'primeng/api';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { SelectButton } from 'primeng/selectbutton';
import { TreeSelect } from 'primeng/treeselect';
import { debounceTime } from 'rxjs';
import { SpecSheetPhase } from '../../../../../../../../core/models/domain/types/spec-sheet-phase';
import { TableOrientation } from '../../../../../../../../core/models/domain/types/spec-sheet-type';
import { PhasesService } from '../../../../../../../../core/services/phases.service';
import { TypesService } from '../../../../../../../../core/services/types.service';
import { ElementLabel } from '../../../../../../../common/element-label/element-label';
import { ManageSpecSheetTypeService } from '../../../services/manage-spec-sheet-type.service';

interface CreateSpecSheetTypeForm {
  type: FormControl<string>;
  tableOrientation: FormControl<TableOrientation>;
  canCopyFromMigrated: FormControl<boolean>;
  requiresApprovalWorkflow: FormControl<boolean>;
  phases: FormControl<TreeNode<SpecSheetPhase>[]>;
}

@Component({
  selector: 'app-create-spec-sheet-type-basic-info',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ElementLabel,
    Message,
    SelectButton,
    TreeSelect,
    InputText,
  ],
  templateUrl: './create-spec-sheet-type-basic-info.html',
  styleUrl: './create-spec-sheet-type-basic-info.scss',
})
export class CreateSpecSheetTypeBasicInfo {
  public readonly form: FormGroup<CreateSpecSheetTypeForm>;
  public readonly phaseOptions = computed<TreeNode<SpecSheetPhase>[]>(() => {
    const phases = this._phasesService.specSheetPhases();

    const nodes: TreeNode[] = [];
    for (const phase of phases) {
      nodes.push({
        key: `${phase.id}`,
        label: phase.phase,
        data: phase,
        children: phase.phases.map((subPhase) => ({
          key: `${subPhase.id}`,
          label: subPhase.phase,
          data: subPhase,
        })),
      });
    }

    return nodes;
  });
  public readonly tableOrientationOptions: {
    label: string;
    value: TableOrientation;
  }[] = [
    { label: 'Horizontal', value: 'horizontal' },
    { label: 'Vertical', value: 'vertical' },
  ];
  public readonly boolOptions = [
    { label: 'Yes', value: true },
    { label: 'No', value: false },
  ];

  private readonly _manageSpecSheetTypeService = inject(
    ManageSpecSheetTypeService,
  );
  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);
  private readonly _formBuilder = inject(NonNullableFormBuilder);

  constructor() {
    this.form = this.buildForm();

    this.form.valueChanges
      .pipe(takeUntilDestroyed(), debounceTime(200))
      .subscribe((value) => {
        const basicInfo = this._manageSpecSheetTypeService.basicInfo();
        basicInfo.title.set(value.type ?? '');
        basicInfo.tableOrientation.set(value.tableOrientation ?? 'vertical');
        basicInfo.canCopyFromMigrated.set(value.canCopyFromMigrated ?? false);
        basicInfo.phases.set(value.phases!.map((node) => node.data!) ?? []);
      });
  }

  public hasError(
    control: keyof CreateSpecSheetTypeForm,
    error: string,
  ): boolean {
    const formControl = this.form.get(control);
    return formControl
      ? formControl.hasError(error) && formControl.touched
      : false;
  }

  private buildForm(): FormGroup<CreateSpecSheetTypeForm> {
    return this._formBuilder.group({
      type: this._formBuilder.control(
        this._manageSpecSheetTypeService.basicInfo().title(),
        [
          Validators.required,
          Validators.minLength(1),
          Validators.maxLength(64),
          this.uniqueTypeValidator(),
        ],
      ),
      tableOrientation: this._formBuilder.control<TableOrientation>(
        this._manageSpecSheetTypeService.basicInfo().tableOrientation(),
        [Validators.required],
      ),
      canCopyFromMigrated: this._formBuilder.control<boolean>(
        this._manageSpecSheetTypeService.basicInfo().canCopyFromMigrated(),
        [Validators.required],
      ),
      requiresApprovalWorkflow: this._formBuilder.control<boolean>(
        this._manageSpecSheetTypeService.basicInfo().requiresApprovalWorkflow(),
        [Validators.required],
      ),
      phases: this._formBuilder.control<TreeNode<SpecSheetPhase>[]>(
        [],
        [Validators.required, Validators.minLength(1)],
      ),
    });
  }

  private uniqueTypeValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const typeCtrl = control as FormControl<string>;

      const rawType = (typeCtrl.value ?? '').trim();
      if (!rawType) {
        return null;
      }

      const normalizedType = rawType.toLowerCase();

      const types = this._typesService.specSheetTypes();
      const duplicate = types.some(
        (type) => type.type.trim().toLowerCase() === normalizedType,
      );

      return duplicate ? { duplicateType: true } : null;
    };
  }
}
