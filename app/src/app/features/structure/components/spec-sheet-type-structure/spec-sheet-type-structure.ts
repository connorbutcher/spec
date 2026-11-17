import { Component, computed, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { Message } from 'primeng/message';
import { StructureLevelType } from '../../../../core/types/configuration/structure-level-type.enum';
import { ManageSpecSheetTypeService } from '../../../admin/features/types/dialogs/services/manage-spec-sheet-type.service';
import { Destroyable } from '../../../common/destructable/destructable';
import { EditSpecSheetTypeStructureLevelGroup } from '../edit-spec-sheet-type-structure-level-group/edit-spec-sheet-type-structure-level-group';
import { EditSpecSheetTypeStructureLevel } from '../edit-spec-sheet-type-structure-level/edit-spec-sheet-type-structure-level';

@Component({
  selector: 'app-spec-sheet-type-structure',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    EditSpecSheetTypeStructureLevel,
    Message,
    ButtonDirective,
    EditSpecSheetTypeStructureLevelGroup,
  ],
  templateUrl: './spec-sheet-type-structure.html',
  styleUrl: './spec-sheet-type-structure.scss',
})
export class SpecSheetTypeStructure extends Destroyable {
  public readonly displayLevel = computed(() =>
    this._manageSpecSheetTypeService
      .structureLevels()
      .levels()
      .find((level) => level.levelType() === StructureLevelType.Display),
  );
  public readonly groupLevels = computed(() =>
    this._manageSpecSheetTypeService
      .structureLevels()
      .levels()
      .filter((level) => level.levelType() === StructureLevelType.Group),
  );
  public readonly hasDuplicatePropertiesAcrossGroups = computed(() =>
    this._manageSpecSheetTypeService
      .structureLevels()
      .hasDuplicatePropertiesAcrossGroups(),
  );

  private readonly _manageSpecSheetTypeService = inject(
    ManageSpecSheetTypeService,
  );

  public addGroup(): void {
    this._manageSpecSheetTypeService.structureLevels().addLevel({
      id: 0,
      level:
        this._manageSpecSheetTypeService.structureLevels().levels().length + 1,
      levelValues: [
        {
          id: 0,
          specSheetTypeStructureLevelId: 0,
          displayOrder: 1,
          specSheetTypePropertyReferenceId: '',
        },
      ],
      sortOrientation: 1,
      specSheetTypeId: this._manageSpecSheetTypeService.basicInfo().id,
      structureLevelType: StructureLevelType.Group,
    });
  }
}
