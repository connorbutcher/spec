import { SheetItemRevision } from '../models/domain/sheets/sheet-item-revision';

export class SheetItemHelpers {
  /** The revision of an item made in the editor, which has not been saved yet. */
  public static newRevision(createdByUserId: number): SheetItemRevision {
    const now = new Date().toISOString();
    return {
      id: 0,
      revisionNumber: 1,
      status: 0,
      createdByUserId,
      createdDateTime: now,
      lastModifiedDateTime: now,
    };
  }
}
