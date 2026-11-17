import {
  Component,
  computed,
  effect,
  inject,
  input,
  OnInit,
  untracked,
} from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  FormsModule,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { Select } from 'primeng/select';
import { SelectButton } from 'primeng/selectbutton';
import { filter, takeUntil } from 'rxjs';
import { SpecSheetTypeStructureLevelValue } from '../../../../core/models/domain/configuration/spec-sheet-type-structure-level-value';
import { SpecSheetTypeProperty } from '../../../../core/models/domain/types/spec-sheet-type-property';
import { ManageSpecSheetTypeService } from '../../../admin/features/types/dialogs/services/manage-spec-sheet-type.service';
import { Destroyable } from '../../../common/destructable/destructable';
import { StructureLevel } from '../../models/structure-level';
import { StructureLevelValue } from '../../models/structure-level-value';

interface SpecSheetTypeStructureLevelForm {
  displayProperties: FormArray<FormGroup<SpecSheetTypeStructureLevelValueForm>>;
  sortOrientation: FormControl<number>;
}

interface SpecSheetTypeStructureLevelValueForm {
  id: FormControl<number>;
  property: FormControl<SpecSheetTypeProperty>;
  order: FormControl<number>;
}

@Component({
  selector: 'app-edit-spec-sheet-type-structure-level',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    Select,
    SelectButton,
    ButtonDirective,
    InputGroup,
    InputGroupAddon,
  ],
  templateUrl: './edit-spec-sheet-type-structure-level.html',
  styleUrl: './edit-spec-sheet-type-structure-level.scss',
})
export class EditSpecSheetTypeStructureLevel
  extends Destroyable
  implements OnInit
{
  public readonly structureLevel = input.required<StructureLevel>();
  public readonly propertyOptions = computed(() =>
    this._manageSpecSheetTypeService
      .properties()
      .properties()
      .map((p) => p.toProperty()),
  );
  public readonly form: FormGroup<SpecSheetTypeStructureLevelForm>;
  public readonly isTabActive = computed(
    () => this._manageSpecSheetTypeService.activeTab() === 'structure',
  );
  private readonly _formBuilder = inject(NonNullableFormBuilder);
  private readonly _manageSpecSheetTypeService = inject(
    ManageSpecSheetTypeService,
  );

  constructor() {
    super();

    this.form = this.buildForm();

    effect(() => {
      const allTemplateProperties = this.propertyOptions();
      if (this.isTabActive()) {
        untracked(() => this.seed());
      }
    });
  }

  ngOnInit(): void {
    this.seed();

    this.form.valueChanges
      .pipe(
        takeUntil(this.destroy),
        filter(() => this.form.valid),
      )
      .subscribe((value) => {
        this.structureLevel().sortOrientation.set(value.sortOrientation!);

        for (const levelValue of value.displayProperties ?? []) {
          const existingLevelValue = this.structureLevel()
            .levelValues()
            .find((lv) => lv.id === levelValue.id);

          if (existingLevelValue) {
            existingLevelValue.displayOrder.set(levelValue.order!);
            existingLevelValue.propertyReferenceId.set(
              levelValue.property!.referenceId,
            );
          } else {
            const newLevelValue = new StructureLevelValue({
              id: 0,
              specSheetTypeStructureLevelId: this.structureLevel().id,
              displayOrder: levelValue.order!,
              specSheetTypePropertyReferenceId:
                levelValue.property!.referenceId,
            });
            this.structureLevel().addLevelValue(newLevelValue);
          }
        }
      });
  }

  public addDisplayProperty(): void {
    const levelValueForm = this.buildLevelValueForm({
      displayOrder: this.displayPropertiesFormArray.length + 1,
      specSheetTypePropertyReferenceId: '',
      id: 0,
      specSheetTypeStructureLevelId: this.structureLevel()?.id ?? 0,
    });
    this.displayPropertiesFormArray.push(levelValueForm);
  }

  public deleteDisplayProperty(index: number): void {
    this.displayPropertiesFormArray.removeAt(index);
  }

  public get displayPropertiesFormArray(): FormArray<
    FormGroup<SpecSheetTypeStructureLevelValueForm>
  > {
    return this.form.get('displayProperties') as FormArray<
      FormGroup<SpecSheetTypeStructureLevelValueForm>
    >;
  }

  public get displayPropertyControlsCount(): number {
    return this.displayPropertiesFormArray.controls.length;
  }

  private seed(): void {
    this.form.controls.sortOrientation.setValue(
      this.structureLevel().sortOrientation(),
    );

    this.displayPropertiesFormArray.clear();

    if (this.propertyOptions().length > 0 && this.structureLevel()) {
      for (const levelValue of this.structureLevel().levelValues()) {
        const levelValueForm = this.buildLevelValueForm(levelValue.toModel());
        this.displayPropertiesFormArray.push(levelValueForm);
      }
    }
  }

  private buildForm(): FormGroup<SpecSheetTypeStructureLevelForm> {
    return this._formBuilder.group<SpecSheetTypeStructureLevelForm>({
      displayProperties: this._formBuilder.array<
        FormGroup<SpecSheetTypeStructureLevelValueForm>
      >([], [Validators.required, Validators.minLength(1)]),
      sortOrientation: this._formBuilder.control<number>(1, [
        Validators.required,
      ]),
    });
  }

  private buildLevelValueForm(
    levelValue: SpecSheetTypeStructureLevelValue,
  ): FormGroup<SpecSheetTypeStructureLevelValueForm> {
    const templateProperty = this.getPropertyForLevel(levelValue);

    return this._formBuilder.group<SpecSheetTypeStructureLevelValueForm>({
      id: this._formBuilder.control<number>(levelValue.id, [
        Validators.required,
      ]),
      property: this._formBuilder.control<SpecSheetTypeProperty>(
        templateProperty!,
        [Validators.required],
      ),
      order: this._formBuilder.control<number>(levelValue.displayOrder, [
        Validators.required,
      ]),
    });
  }

  private getPropertyForLevel(
    levelValue: SpecSheetTypeStructureLevelValue | undefined,
  ): SpecSheetTypeProperty | undefined {
    if (!levelValue) {
      return undefined;
    }

    return this.propertyOptions().find(
      (tp) => tp.referenceId === levelValue.specSheetTypePropertyReferenceId,
    );
  }
}
