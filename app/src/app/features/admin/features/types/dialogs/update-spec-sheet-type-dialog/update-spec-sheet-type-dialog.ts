import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, computed, inject, signal, Signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MessageService, TreeNode } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { UpdateSpecSheetTypeRequest } from '../../../../../../core/models/requests/types/type/update-spec-sheet-type-request';
import { PhasesService } from '../../../../../../core/services/phases.service';
import { TypesService } from '../../../../../../core/services/types.service';
import { DialogHeader } from '../../../../../common/dialog-header/dialog-header';
import { PropertyConstants } from '../../../../../properties/constants/property-constants';
import { Properties } from '../../../../../properties/models/properties.class';
import { StructureLevels } from '../../../../../structure/models/structure-levels';
import { TypesDataService } from '../../types-data.service';
import { SpecSheetTypeBasicInfo } from '../models/spec-sheet-type-basic-info';
import { ManageSpecSheetTypeService } from '../services/manage-spec-sheet-type.service';
import { UpdateSpecSheetTypeBasicInfo } from './components/update-spec-sheet-type-basic-info/update-spec-sheet-type-basic-info';
import { UpdateSpecSheetTypeProperties } from './components/update-spec-sheet-type-properties/update-spec-sheet-type-properties';
import { UpdateSpecSheetTypeStructure } from './components/update-spec-sheet-type-structure/update-spec-sheet-type-structure';
import { UpdateSpecSheetTypeDialogData } from './models/update-spec-sheet-type-dialog-data';

@Component({
  selector: 'app-create-spec-sheet-type-dialog',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ButtonDirective,
    DialogHeader,
    TabsModule,
    UpdateSpecSheetTypeBasicInfo,
    UpdateSpecSheetTypeProperties,
    UpdateSpecSheetTypeStructure,
  ],
  templateUrl: './update-spec-sheet-type-dialog.html',
  styleUrl: './update-spec-sheet-type-dialog.scss',
  host: { class: 'dialog card' },
  providers: [ManageSpecSheetTypeService],
})
export class UpdateSpecSheetTypeDialog {
  public readonly isSaving = signal(false);
  public readonly phaseOptions = computed<TreeNode<SpecSheetPhase>[]>(() => {
    const phases = this._phasesService.specSheetPhases();

    const nodes: TreeNode<SpecSheetPhase>[] = [];
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

  private readonly _typesService = inject(TypesService);
  private readonly _phasesService = inject(PhasesService);
  private readonly _typesDataService = inject(TypesDataService);
  private readonly _dialogRef = inject(DialogRef<UpdateSpecSheetTypeDialog>);
  private readonly _dialogData =
    inject<UpdateSpecSheetTypeDialogData>(DIALOG_DATA);
  private readonly _messageService = inject(MessageService);
  private readonly _manageSpecSheetTypeService = inject(
    ManageSpecSheetTypeService,
  );

  constructor() {
    this._manageSpecSheetTypeService.basicInfo.set(
      new SpecSheetTypeBasicInfo(this._dialogData.type),
    );
    this._manageSpecSheetTypeService.properties.set(
      new Properties([
        ...PropertyConstants.defaultRequiredProperties.map((dp) => ({
          ...dp,
          specSheetTypeId: this._dialogData.type.id,
        })),
        ...this._dialogData.type.properties,
      ]),
    );
    this._manageSpecSheetTypeService.structureLevels.set(
      new StructureLevels(this._dialogData.type.structureLevels),
    );
  }

  public async updateAsync(): Promise<void> {
    if (!this.isValid() || this.isSaving()) {
      this._messageService.add({
        severity: 'error',
        summary: 'Invalid Input',
        detail: 'Please correct the errors in the form before submitting.',
      });
      return;
    }

    this.isSaving.set(true);

    try {
      const basicInfo = this._manageSpecSheetTypeService.basicInfo();

      const newSpecSheetPhaseIds = basicInfo.getNewPhases();
      const removedSpecSheetPhaseIds = basicInfo.getRemovedPhases();
      const structure = this._manageSpecSheetTypeService
        .structureLevels()
        .toManageStructureLevelsRequest();
      const properties = this._manageSpecSheetTypeService
        .properties()
        .toManagePropertiesRequest();

      const request = {
        id: this._dialogData.type.id,
        type: basicInfo.title(),
        displayOrder: this._dialogData.type.displayOrder,
        canCopyFromMigrated: basicInfo.canCopyFromMigrated(),
        requiresApprovalWorkflow: basicInfo.requiresApprovalWorkflow(),
        newSpecSheetPhaseIds,
        removedSpecSheetPhaseIds,
        properties,
        structure,
      } as UpdateSpecSheetTypeRequest;
      await this._typesDataService.updateSpecSheetTypeAsync(request);

      this._dialogRef.close();
      this._messageService.add({
        severity: 'success',
        summary: 'Update Successful',
        detail: 'The spec sheet type has been updated successfully.',
      });
      this._typesService.refresh();
      this._phasesService.refresh();
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Update Failed',
        detail: 'An error occurred while updating the spec sheet type.',
      });
    } finally {
      this.isSaving.set(false);
    }
  }

  public tabChange(value: any): void {
    this._manageSpecSheetTypeService.activeTab.set(value);
  }

  public close(): void {
    this._dialogRef.close();
  }

  public get isValid(): Signal<boolean> {
    return this._manageSpecSheetTypeService.isValid;
  }

  public get hasChanges(): Signal<boolean> {
    return this._manageSpecSheetTypeService.hasChanges;
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
