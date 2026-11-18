import { Component, computed, effect, input, Signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { DateTime } from 'luxon';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Textarea } from 'primeng/textarea';
import { nextTempId } from '../../../../../../core/helpers/temp-id-counter';
import { UserService } from '../../../../../../core/services/user.service';
import { PUSpecSheetTableNote } from '../../../../models/pu-spec-sheet-table-note';
import { PUSpecSheetTableBase } from '../../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../../services/active-sheet.service';
import { ViewNote } from '../view-note/view-note';

@Component({
  selector: 'app-view-notes',
  imports: [ReactiveFormsModule, ButtonDirective, Textarea, ViewNote],
  templateUrl: './view-notes.html',
  styleUrl: './view-notes.scss',
})
export class ViewNotes {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly tabCtrl = new FormControl<string>('', [Validators.required]);
  public readonly notes = computed(() => this.table().notes());

  constructor(
    private readonly _sheetService: ActiveSheetService,
    private readonly _toastService: MessageService,
    private readonly _userService: UserService,
  ) {
    effect(() => {
      const canEdit = this._sheetService.canEdit();
      if (!canEdit) {
        this.tabCtrl.disable({ emitEvent: false });
      } else {
        this.tabCtrl.enable({ emitEvent: false });
      }
    });
  }

  public addNote(): void {
    const note = this.tabCtrl.value;
    if (!this.canEdit() || !note) {
      return;
    }

    const user = this._userService.user();
    if (!user) {
      return;
    }

    this.table().addNote({
      id: nextTempId(),
      createdByUserId: user.id,
      createdDateTime: DateTime.now().toISO(),
      note,
      createdByUser: user,
      isObsolete: false,
      sourceSystem: 'puspecsheet',
      sheetTableId: this.table().id,
    });

    this.tabCtrl.reset();
  }

  public deleteNote(noteToDelete: PUSpecSheetTableNote): void {
    if (!this.canEdit() || !noteToDelete) {
      return;
    }

    this.table().removeNote(noteToDelete);
  }

  public get canEdit(): Signal<boolean> {
    return this._sheetService.canEdit;
  }
}
