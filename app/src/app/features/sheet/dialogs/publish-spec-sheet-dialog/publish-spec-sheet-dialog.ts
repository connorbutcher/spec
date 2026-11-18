import { DialogRef } from '@angular/cdk/dialog';
import { Component, computed, inject, Signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { Message } from 'primeng/message';
import { Select } from 'primeng/select';
import { SpecSheetPhaseObserver } from '../../../../core/models/domain/access/spec-sheet-phase-observer';
import { PublishSheetRequest } from '../../../../core/models/requests/sheets/publish-sheet-request';
import { PhasesService } from '../../../../core/services/phases.service';
import { UserService } from '../../../../core/services/user.service';
import { DialogHeader } from '../../../common/dialog-header/dialog-header';
import { ElementLabel } from '../../../common/element-label/element-label';
import { RichTextEditor } from '../../../common/rich-text-editor/rich-text-editor';
import { SheetsService } from '../../../sheets/services/sheets.service';
import { PUSpecSheet } from '../../models/pu-spec-sheet';
import { ActiveSheetService } from '../../services/active-sheet.service';

interface PublishSpecSheetDialogForm {
  publishInfo: FormControl<string>;
  approvers: FormControl<SpecSheetPhaseObserver[]>;
}

@Component({
  selector: 'app-publish-spec-sheet-dialog',
  imports: [
    ReactiveFormsModule,
    DialogHeader,
    ElementLabel,
    ButtonDirective,
    RichTextEditor,
    Select,
    Message,
  ],
  templateUrl: './publish-spec-sheet-dialog.html',
  styleUrl: './publish-spec-sheet-dialog.scss',
  host: { class: 'dialog card' },
})
export class PublishSpecSheetDialog {
  public readonly form: FormGroup<PublishSpecSheetDialogForm>;
  public readonly approverOptions = computed(() => {
    const phase = this._phasesService.selectedPhase();
    const phases = this._phasesService.specSheetPhases();
    const currentUser = this._userService.user();
    if (!phase || !phases || !currentUser) {
      return [];
    }

    const observerPhase = phases.find(
      (p) => p.id === (phase.parentSpecSheetPhaseId ?? phase.id),
    );
    if (!observerPhase) {
      return [];
    }

    return observerPhase.phaseObservers.filter(
      (po) => po.isApprover && po.user.id !== currentUser.id,
    );
  });
  public readonly requiresApprovalWorkflow = computed(
    () => this._sheetsService.selectedType()?.requiresApprovalWorkflow ?? false,
  );

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sheetsService = inject(SheetsService);
  private readonly _phasesService = inject(PhasesService);
  private readonly _userService = inject(UserService);
  private readonly _dialogRef = inject(
    DialogRef<PublishSheetRequest, PublishSpecSheetDialog>,
  );
  private readonly _formBuilder = inject(NonNullableFormBuilder);

  constructor() {
    this.form = this.buildForm();

    if (!this.requiresApprovalWorkflow()) {
      this.form.controls.approvers.clearValidators();
    }
  }

  public publish(): void {
    const request = {
      id: this.specSheet().id,
      changeLog: this.form.value.publishInfo,
      approvers: this.form.value.approvers?.map((o) => o.user) ?? [],
    } as PublishSheetRequest;

    this._dialogRef.close(request);
  }

  public compareSpecSheets(): void {
    const specSheet = this.specSheet();
    if (!specSheet) {
      return;
    }

    const versions = this._sheetsService.specSheetsForPhaseAndType();
    if (versions.length === 0) {
      return;
    }

    const indexOfCurrentVersion = versions.findIndex(
      (v) => v.versionNumber === specSheet.versionNumber(),
    );
    if (
      indexOfCurrentVersion === -1 ||
      indexOfCurrentVersion === versions.length - 1
    ) {
      return;
    }

    const previousVersion = versions[indexOfCurrentVersion + 1];
    if (!previousVersion) {
      return;
    }

    const currentVersion = specSheet;
    const url = `${window.location.origin}/comparison?type=${currentVersion.specSheetType().id}&phase=${currentVersion.specSheetPhase().id}&left=${previousVersion.versionNumber}&right=${currentVersion.versionNumber}`;
    window.open(url, '_blank');
  }

  public close(): void {
    this._dialogRef.close(undefined);
  }

  public hasError(
    controlName: keyof PublishSpecSheetDialogForm,
    error: string,
  ): boolean {
    const control = this.form.get(controlName);
    return !!control && control.dirty && control.hasError(error);
  }

  public get specSheet(): Signal<PUSpecSheet> {
    return this._activeSheetService.specSheet;
  }

  private buildForm(): FormGroup<PublishSpecSheetDialogForm> {
    return this._formBuilder.group({
      publishInfo: this._formBuilder.control<string>('', [Validators.required]),
      approvers: this._formBuilder.control<SpecSheetPhaseObserver[]>(
        [],
        [Validators.required, Validators.minLength(1)],
      ),
    });
  }
}
