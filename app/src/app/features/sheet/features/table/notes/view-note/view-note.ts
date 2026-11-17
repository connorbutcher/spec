import { DatePipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { PUSpecSheetTableNote } from '../../../../models/pu-spec-sheet-table-note';
import { ActiveSheetService } from '../../../../services/active-sheet.service';

@Component({
  selector: 'app-view-note',
  imports: [DatePipe, ButtonDirective, Tooltip],
  templateUrl: './view-note.html',
  styleUrl: './view-note.scss',
})
export class ViewNote {
  public readonly note = input.required<PUSpecSheetTableNote>();
  public readonly isLast = input.required<boolean>();
  public readonly removeNote = output<PUSpecSheetTableNote>();

  public readonly canMarkAsObsolete = computed(() => {
    const note = this.note();

    const canEdit = this._sheetService.canEdit();
    if (!note || !canEdit) {
      return false;
    }

    return (
      !note.isObsolete() && note.sourceSystem() === 'puspecsheet' && !note.isNew
    );
  });
  public readonly canDeleteNote = computed(() => {
    const note = this.note();

    const canEdit = this._sheetService.canEdit();
    if (!note || !canEdit) {
      return false;
    }

    return note.sourceSystem() === 'pucards' || note.isNew;
  });

  constructor(private readonly _sheetService: ActiveSheetService) {}

  public markNoteAsObsolete(): void {
    const note = this.note();
    if (!note || !this.canMarkAsObsolete()) {
      return;
    }

    this.note().isObsolete.set(true);
  }

  public deleteNote(): void {
    const note = this.note();
    if (!note || !this.canDeleteNote()) {
      return;
    }

    this.removeNote.emit(note);
  }
}
