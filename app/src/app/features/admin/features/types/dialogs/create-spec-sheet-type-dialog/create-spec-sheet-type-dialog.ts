import { DialogRef } from '@angular/cdk/dialog';
import { Component, inject, signal, Signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { StepperModule } from 'primeng/stepper';
import { CreateSpecSheetTypeRequest } from '../../../../../../core/models/requests/types/type/create-spec-sheet-type-request';
import { TypesService } from '../../../../../../core/services/types.service';
import { StructureLevelType } from '../../../../../../core/types/configuration/structure-level-type.enum';
import { DialogHeader } from '../../../../../common/dialog-header/dialog-header';
import { PropertyConstants } from '../../../../../properties/constants/property-constants';
import { Properties } from '../../../../../properties/models/properties.class';
import { StructureLevels } from '../../../../../structure/models/structure-levels';
import { TypesDataService } from '../../types-data.service';
import { SpecSheetTypeBasicInfo } from '../models/spec-sheet-type-basic-info';
import { ManageSpecSheetTypeService } from '../services/manage-spec-sheet-type.service';
import { CreateSpecSheetTypeBasicInfo } from './components/create-spec-sheet-type-basic-info/create-spec-sheet-type-basic-info';
import { CreateSpecSheetTypeProperties } from './components/create-spec-sheet-type-properties/create-spec-sheet-type-properties';
import { CreateSpecSheetTypeStructure } from './components/create-spec-sheet-type-structure/create-spec-sheet-type-structure';

@Component({
  selector: 'app-create-spec-sheet-type-dialog',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ButtonDirective,
    DialogHeader,
    StepperModule,
    CreateSpecSheetTypeProperties,
    CreateSpecSheetTypeBasicInfo,
    CreateSpecSheetTypeStructure,
  ],
  templateUrl: './create-spec-sheet-type-dialog.html',
  styleUrl: './create-spec-sheet-type-dialog.scss',
  host: { class: 'dialog card' },
  providers: [ManageSpecSheetTypeService],
})
export class CreateSpecSheetTypeDialog {
  public readonly isCreating = signal(false);

  private readonly _typesService = inject(TypesService);
  private readonly _phaseDataService = inject(TypesDataService);
  private readonly _dialogRef = inject(DialogRef<CreateSpecSheetTypeDialog>);
  private readonly _messageService = inject(MessageService);
  private readonly _manageSpecSheetTypeService = inject(
    ManageSpecSheetTypeService,
  );

  constructor() {
    this._manageSpecSheetTypeService.basicInfo.set(
      new SpecSheetTypeBasicInfo({
        id: 0,
        type: '',
        tableOrientation: 'vertical',
        displayOrder: 0,
        canCopyFromMigrated: false,
        requiresApprovalWorkflow: true,
        tableTemplates: [],
        permissions: [],
        properties: [],
        structureLevels: [],
      }),
    );
    this._manageSpecSheetTypeService.properties.set(
      new Properties(PropertyConstants.defaultProperties),
    );
    this._manageSpecSheetTypeService.structureLevels.set(
      new StructureLevels([
        {
          id: 0,
          structureLevelType: StructureLevelType.Display,
          level: 1,
          sortOrientation: 1,
          specSheetTypeId: 0,
          levelValues: [],
        },
      ]),
    );
  }

  public async createAsync(): Promise<void> {
    if (!this.isValid() || this.isCreating()) {
      this._messageService.add({
        severity: 'error',
        summary: 'Invalid Input',
        detail: 'Please correct the errors in the form before submitting.',
      });
      return;
    }

    this.isCreating.set(true);

    try {
      const displayOrder = this._typesService.specSheetTypes().length + 1;

      const basicInfo = this._manageSpecSheetTypeService.basicInfo();
      const properties = this._manageSpecSheetTypeService
        .properties()
        .getNewProperties();
      const structureLevels = this._manageSpecSheetTypeService
        .structureLevels()
        .getNewLevels();

      const request = {
        type: basicInfo.title(),
        tableOrientation: basicInfo.tableOrientation(),
        displayOrder,
        canCopyFromMigrated: basicInfo.canCopyFromMigrated(),
        requiresApprovalWorkflow: basicInfo.requiresApprovalWorkflow(),
        specSheetPhaseIds: basicInfo.phases().map((phase) => phase.id),
        properties,
        structureLevels,
      } as CreateSpecSheetTypeRequest;
      await this._phaseDataService.createSpecSheetTypeAsync(request);

      this._dialogRef.close();
      this._messageService.add({
        severity: 'success',
        summary: 'Type Created',
        detail: `The spec sheet type has been created successfully.`,
      });
      this._typesService.refresh();
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Creation Failed',
        detail: 'An error occurred while creating the spec sheet type.',
      });
    } finally {
      this.isCreating.set(false);
    }
  }

  public stepChange(value: number | undefined): void {
    let tab: 'general' | 'properties' | 'structure';
    switch (value) {
      case 1:
      default:
        tab = 'general';
        break;
      case 2:
        tab = 'properties';
        break;
      case 3:
        tab = 'structure';
        break;
    }

    this._manageSpecSheetTypeService.activeTab.set(tab);
  }

  public close(): void {
    this._dialogRef.close();
  }

  public get isValid(): Signal<boolean> {
    return this._manageSpecSheetTypeService.isValid;
  }

  public get basicInfoValid(): Signal<boolean> {
    return this._manageSpecSheetTypeService.basicInfoValid;
  }

  public get propertiesValid(): Signal<boolean> {
    return this._manageSpecSheetTypeService.propertiesValid;
  }

  public get structureValid(): Signal<boolean> {
    return this._manageSpecSheetTypeService.structureValid;
  }
}
