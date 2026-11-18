import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import {
  AfterViewInit,
  Component,
  computed,
  inject,
  Signal,
  signal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import {
  takeUntilDestroyed,
  toObservable,
  toSignal,
} from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  FormsModule,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MessageService, TreeNode } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Select } from 'primeng/select';
import { TreeSelect } from 'primeng/treeselect';
import {
  combineLatest,
  distinctUntilChanged,
  filter,
  map,
  take,
  takeUntil,
} from 'rxjs';
import { SpecSheetPhase } from '../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../core/models/domain/types/spec-sheet-type';
import { SpecSheetTypeProperty } from '../../../../core/models/domain/types/spec-sheet-type-property';
import { CreateSheetRequest } from '../../../../core/models/requests/sheets/create-sheet-request';
import { CreateSheetPropertyValueRequest } from '../../../../core/models/requests/sheets/create-sheet-property-value-request';
import { PhasesService } from '../../../../core/services/phases.service';
import { TypesService } from '../../../../core/services/types.service';
import { PropertyControlType } from '../../../../core/types/properties/property-control-type.enum';
import { Destroyable } from '../../../common/destructable/destructable';
import { DialogHeader } from '../../../common/dialog-header/dialog-header';
import { ElementLabel } from '../../../common/element-label/element-label';
import { PropertyControlBase } from '../../../properties/components/controls/property-control-base';
import { PropertyControlTypeHelpers } from '../../../properties/helpers/property-control-type-helpers';
import { PropertiesInfoService } from '../../../properties/services/properties-info.service';
import { CreatePUSpecSheetDialogDataService } from './create-pu-spec-sheet-dialog-data.service';
import { CreatePUSpecSheetDialogData } from './models/create-pu-spec-sheet-dialog-data';

@Component({
  selector: 'app-create-pu-spec-sheet-dialog',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    Select,
    TreeSelect,
    DialogHeader,
    ButtonDirective,
    ElementLabel,
  ],
  templateUrl: './create-pu-spec-sheet-dialog.html',
  styleUrl: './create-pu-spec-sheet-dialog.scss',
  providers: [CreatePUSpecSheetDialogDataService, PropertiesInfoService],
  host: { class: 'dialog card' },
})
export class CreatePuSpecSheetDialog
  extends Destroyable
  implements AfterViewInit
{
  public readonly form: FormGroup;
  public readonly formContent = viewChild.required('formContent', {
    read: ViewContainerRef,
  });
  public readonly formComponents = signal<PropertyControlBase<any, any>[]>([]);
  public readonly isFormControlsValid = computed(() =>
    this.formComponents().every((c) => c.isValid()),
  );
  public readonly isCreating = signal(false);
  public readonly hasErrored = signal<boolean>(false);
  public readonly errorMessage = signal<string>('');
  public readonly phaseOptions: Signal<TreeNode<SpecSheetPhase>[]>;

  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);
  private readonly _createPUSpecSheetDialogDataService = inject(
    CreatePUSpecSheetDialogDataService,
  );
  private readonly _dialogRef = inject(DialogRef);
  private readonly _dialogData =
    inject<CreatePUSpecSheetDialogData>(DIALOG_DATA);
  private readonly _formBuilder = inject(NonNullableFormBuilder);
  private readonly _messageService = inject(MessageService);
  private readonly _propertiesInfoService = inject(PropertiesInfoService);

  constructor() {
    super();

    this._propertiesInfoService.addExtraInfo(
      'copyFromSpecSheetId',
      this._dialogData.copyFromSpecSheetId,
    );

    this.form = this.buildForm();

    this.specSheetPhaseCtrl.valueChanges
      .pipe(
        takeUntilDestroyed(),
        filter((specSheetPhaseValue) => !!specSheetPhaseValue),
      )
      .subscribe((specSheetPhase) => {
        this._propertiesInfoService.addExtraInfo(
          'specSheetPhase',
          specSheetPhase.data!,
        );
      });

    this.phaseOptions = toSignal(
      combineLatest([
        this.specSheetTypeCtrl.valueChanges,
        toObservable(this._phasesService.specSheetPhases),
      ]).pipe(
        takeUntilDestroyed(),
        distinctUntilChanged(),
        filter(([type]) => !!type),
        map(([type, phases]) => this.buildPhaseOptions(type!, phases)),
      ),
      { initialValue: [] },
    );

    if (this._dialogData.phase) {
      toObservable(this.phaseOptions)
        .pipe(
          filter((options) => options.length > 0),
          take(1),
        )
        .subscribe((options) => {
          const phaseNode = this.findPhaseNodeById(
            options,
            this._dialogData.phase!.id,
          );
          if (phaseNode) {
            this.specSheetPhaseCtrl.setValue(phaseNode);
          }

          this.specSheetPhaseCtrl.disable();
        });
    }
  }

  ngAfterViewInit(): void {
    this.specSheetTypeCtrl.valueChanges
      .pipe(
        takeUntil(this.destroy),
        filter((specSheetTypeValue) => !!specSheetTypeValue),
      )
      .subscribe((specSheetType) => {
        this._propertiesInfoService.addExtraInfo(
          'specSheetType',
          specSheetType,
        );

        for (const component of this.formComponents()) {
          this.form.removeControl(component.property().valueKey);
        }
        this.formContent().clear();
        this.formComponents.set([]);

        this.hasErrored.set(false);

        for (const property of this.getFormProperties(specSheetType).sort(
          (a, b) => a.displayOrder - b.displayOrder,
        )) {
          if (!this.form.contains(property.valueKey)) {
            this.form.addControl(
              property.valueKey,
              this._formBuilder.control(undefined),
            );
          }

          this.addComponent(property);
        }
      });

    if (this._dialogData.type) {
      const type = this.specSheetTypes().find(
        (t) => t.id === this._dialogData.type!.id,
      );
      this.specSheetTypeCtrl.setValue(type!);
      this.specSheetTypeCtrl.disable();
    }
  }

  public async createAsync(): Promise<void> {
    if (this.form.invalid || this.hasErrored() || this.isCreating()) {
      return;
    }

    this.isCreating.set(true);

    try {
      const propertyValues: CreateSheetPropertyValueRequest[] = [];
      for (const property of this.getFormProperties(
        this.specSheetTypeCtrl.value,
      )) {
        const control = this.form.get(property.valueKey);
        if (control) {
          const value = control.value;
          propertyValues.push({
            specSheetTypePropertyId: property.id,
            propertyValue: value,
          });
        }
      }

      const request = {
        specSheetPhaseId: this.specSheetPhaseCtrl.value.data!.id,
        specSheetTypeId: this.specSheetTypeCtrl.value.id,
        propertyValues,
      } as CreateSheetRequest;

      const specSheetId =
        await this._createPUSpecSheetDialogDataService.createSpecSheetAsync(
          request,
        );

      this._messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Spec Sheet created successfully.',
      });
      this._dialogRef.close(specSheetId);
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'An error occurred while creating the Spec Sheet.',
      });
    } finally {
      this.isCreating.set(false);
    }
  }

  public close(): void {
    this._dialogRef.close();
  }

  public hasError(controlName: string, errorName: string): boolean {
    const control = this.form.controls[controlName];
    return control.hasError(errorName) && control.dirty;
  }

  /**
   * The type's properties that get a control in the form below the type and phase. The type and phase properties
   * are set by the dialog's own type and phase controls, so they have no edit component and are left out.
   */
  private getFormProperties(
    specSheetType: SpecSheetType,
  ): SpecSheetTypeProperty[] {
    return (specSheetType.properties ?? []).filter(
      (property) =>
        property.canValueBeSetInForm &&
        PropertyControlTypeHelpers.getEditComponent(
          property.valueControlType,
        ) !== undefined,
    );
  }

  private buildForm(): FormGroup {
    return this._formBuilder.group({
      type: this._formBuilder.control<SpecSheetType>(undefined!, [
        Validators.required,
      ]),
      phase: this._formBuilder.control<TreeNode<SpecSheetPhase>>(undefined!, [
        Validators.required,
      ]),
    });
  }

  public get specSheetTypes(): Signal<SpecSheetType[]> {
    return this._typesService.specSheetTypes;
  }

  private addComponent(property: SpecSheetTypeProperty): void {
    const formContent = this.formContent();
    if (!formContent) {
      return;
    }

    const component = formContent.createComponent<
      PropertyControlBase<any, any>
    >(
      PropertyControlTypeHelpers.getEditComponent(
        property.valueControlType as PropertyControlType,
      ),
    );

    component.setInput('property', property);
    component.setInput(
      'control',
      this.form.get(property.valueKey) as FormControl,
    );

    this.formComponents.update((components) => [
      ...components,
      component.instance,
    ]);
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
        selectable: phaseHasType && !phase.isObsolete,
        styleClass:
          !phase.isObsolete && (phaseHasType || subPhaseHasType)
            ? ''
            : 'p-disabled',
        children: phase.phases.map((subPhase) => {
          const subPhaseHasType = subPhase.types.some((t) => t.id === type.id);

          return {
            key: `${subPhase.id}`,
            label: subPhase.phase,
            data: subPhase,
            selectable: subPhaseHasType && !subPhase.isObsolete,
            styleClass:
              subPhaseHasType && !subPhase.isObsolete ? '' : 'p-disabled',
          };
        }),
      });
    }

    return nodes;
  }

  private findPhaseNodeById(
    nodes: TreeNode<SpecSheetPhase>[],
    phaseId: number,
  ): TreeNode<SpecSheetPhase> | null {
    for (const node of nodes) {
      if (node.data?.id === phaseId) {
        return node;
      }

      const children = node.children as TreeNode<SpecSheetPhase>[] | undefined;
      if (children?.length) {
        const match = this.findPhaseNodeById(children, phaseId);
        if (match) {
          return match;
        }
      }
    }

    return null;
  }

  private get specSheetTypeCtrl(): FormControl<SpecSheetType> {
    return this.form.controls['type'] as FormControl<SpecSheetType>;
  }

  private get specSheetPhaseCtrl(): FormControl<TreeNode<SpecSheetPhase>> {
    return this.form.controls['phase'] as FormControl<TreeNode<SpecSheetPhase>>;
  }
}
