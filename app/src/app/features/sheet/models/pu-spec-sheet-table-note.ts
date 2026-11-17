import { computed, Signal, signal, WritableSignal } from '@angular/core';
import { UpdateHelpers } from '../../../core/helpers/update-helpers';
import { User } from '../../../core/models/domain/access/user';
import {
  NoteSourceSystem,
  SpecSheetTableNote,
} from '../../../core/models/domain/spec-sheet/spec-sheet-table-note';
import { SpecSheetOp } from '../../../core/models/operations/spec-sheet-op';
import { CreateSpecSheetTableNoteRequest } from '../../../core/models/requests/spec-sheets/notes/create-spec-sheet-table-note-request';
import { UpdateSpecSheetTableNoteRequest } from '../../../core/models/requests/spec-sheets/notes/update-spec-sheet-table-note-request';
import { ChangeTracker } from './services/change-tracker';

export class PUSpecSheetTableNote {
  public readonly id: number;
  public readonly note: WritableSignal<string>;
  public readonly sourceSystem: WritableSignal<NoteSourceSystem>;
  public readonly isObsolete: WritableSignal<boolean>;
  public readonly createdByUser: WritableSignal<User>;
  public readonly createdDateTime: WritableSignal<string>;

  public readonly hasChanges: Signal<boolean>;

  public readonly isValid = computed(() => this.advisories().length === 0);
  public readonly advisories = computed(() => {
    const advisories: string[] = [];

    if (this.note().trim().length === 0) {
      advisories.push('Note is required');
    }

    if (this.sourceSystem() === null) {
      advisories.push('Source system is required');
    }

    return advisories;
  });

  constructor(public readonly specSheetTableNote: SpecSheetTableNote) {
    this.id = specSheetTableNote.id;
    this.note = signal(specSheetTableNote.note);
    this.sourceSystem = signal(specSheetTableNote.sourceSystem);
    this.isObsolete = signal(specSheetTableNote.isObsolete);
    this.createdByUser = signal(specSheetTableNote.createdByUser);
    this.createdDateTime = signal(specSheetTableNote.createdDateTime);
    this.hasChanges = new ChangeTracker(specSheetTableNote).track(
      'isObsolete',
      this.isObsolete,
    ).hasChanges;
  }

  public toNewNoteRequest(): CreateSpecSheetTableNoteRequest {
    return {
      specSheetTableId: this.specSheetTableNote.specSheetTableId,
      note: this.note(),
      sourceSystem: this.sourceSystem(),
      createdByUserId: this.createdByUser().id,
      createdDateTime: this.createdDateTime(),
    };
  }

  public toUpdatedNoteRequest(): UpdateSpecSheetTableNoteRequest {
    return {
      id: this.id,
      isObsolete: UpdateHelpers.compare(
        this.specSheetTableNote.isObsolete,
        this.isObsolete(),
      ),
    };
  }

  public *compileOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      yield {
        $type: 'AddSpecSheetTableNoteOp',
        specSheetTableId: this.specSheetTableNote.specSheetTableId,
        note: this.note(),
        sourceSystem: this.sourceSystem(),
      };
    } else if (this.hasChanges()) {
      yield {
        $type: 'UpdateSpecSheetTableNoteOp',
        id: this.id,
        isObsolete: UpdateHelpers.compare(
          this.specSheetTableNote.isObsolete,
          this.isObsolete(),
        ),
      };
    }
  }

  public get isNew(): boolean {
    return this.id <= 0;
  }
}
