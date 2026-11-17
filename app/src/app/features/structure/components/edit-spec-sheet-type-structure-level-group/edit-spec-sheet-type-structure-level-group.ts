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
  FormControl,
  FormGroup,
  FormsModule,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { Select } from 'primeng/select';
import { SelectButton } from 'primeng/selectbutton';
import { filter, takeUntil } from 'rxjs';
import { SpecSheetTypeProperty } from '../../../../core/models/domain/types/spec-sheet-type-property';
import { ManageSpecSheetTypeService } from '../../../admin/features/types/dialogs/services/manage-spec-sheet-type.service';
import { Destroyable } from '../../../common/destructable/destructable';
import { StructureLevel } from '../../models/structure-level';

interface PUCardTypeStructureLevelGroupForm {
  property: FormControl<SpecSheetTypeProperty | undefined>;
  sortOrientation: FormControl<number>;
}

@Component({
  selector: 'app-edit-spec-sheet-type-structure-level-group',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    Select,
    SelectButton,
    ButtonDirective,
  ],
  templateUrl: './edit-spec-sheet-type-structure-level-group.html',
  styleUrl: './edit-spec-sheet-type-structure-level-group.scss',
})
export class EditSpecSheetTypeStructureLevelGroup
  extends Destroyable
  implements OnInit
{
  public readonly structureLevelGroup = input.required<StructureLevel>();
  public readonly propertyOptions = computed(() =>
    this._manageSpecSheetTypeService
      .properties()
      .properties()
      .filter((p) => p.canBeEdited())
      .map((p) => p.toProperty()),
  );
  public readonly isTabActive = computed(
    () => this._manageSpecSheetTypeService.activeTab() === 'structure',
  );

  public readonly form: FormGroup<PUCardTypeStructureLevelGroupForm>;

  private readonly _manageSpecSheetTypeService = inject(
    ManageSpecSheetTypeService,
  );
  private readonly _formBuilder = inject(NonNullableFormBuilder);

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
        this.structureLevelGroup().sortOrientation.set(value.sortOrientation!);

        const property = value.property;
        const levelValue = this.structureLevelGroup().levelValues()[0];
        if (property && levelValue) {
          levelValue.propertyReferenceId.set(property.referenceId);
        }
      });
  }

  public removeGroup(): void {
    this._manageSpecSheetTypeService
      .structureLevels()
      .removeLevel(this.structureLevelGroup());
  }

  private seed(): void {
    const level = this.structureLevelGroup();
    if (!this.form.value.property && level) {
      const property = this.getPropertyForLevel(level);
      if (property) {
        this.form.patchValue({ property: property });
      }
    }
  }

  private getPropertyForLevel(
    level: StructureLevel | undefined,
  ): SpecSheetTypeProperty | undefined {
    if (!level) {
      return undefined;
    }

    // We can assume there is only level value as groups cannot have multiple level values.
    const levelValue = level.levelValues()[0];
    if (!levelValue) {
      return undefined;
    }

    return this.propertyOptions().find(
      (tp) => tp.referenceId === levelValue.propertyReferenceId(),
    );
  }

  private buildForm(): FormGroup<PUCardTypeStructureLevelGroupForm> {
    return this._formBuilder.group<PUCardTypeStructureLevelGroupForm>({
      property: this._formBuilder.control<SpecSheetTypeProperty | undefined>(
        undefined,
        [Validators.required],
      ),
      sortOrientation: this._formBuilder.control<number>(1, [
        Validators.required,
      ]),
    });
  }
}
