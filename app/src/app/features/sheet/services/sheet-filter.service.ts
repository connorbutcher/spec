import { computed, Injectable, signal } from '@angular/core';
import { orderBy } from 'lodash';

export type Match = {
  referenceId: string;
  tableOrder: number;
  groupOrder: number;
  rowNumber: number;
};

@Injectable()
export class SheetFilterService {
  public readonly filter = signal<string>('');
  public readonly activeFilterResultReferenceId = signal<string | undefined>(
    undefined,
  );
  public readonly matches = computed(() =>
    orderBy(
      this.matchReferenceIds(),
      ['tableOrder', 'groupOrder', 'rowNumber'],
      ['asc', 'asc', 'asc'],
    ),
  );

  public readonly matchReferenceIds = signal<Match[]>([]);

  public addMatch(match: Match): void {
    this.matchReferenceIds.update((ids) => {
      // Avoid duplicates
      if (ids.find((m) => m.referenceId === match.referenceId)) {
        return ids;
      }

      return [...ids, match];
    });
  }

  public removeMatch(referenceId: string): void {
    this.matchReferenceIds.update((ids) => {
      return ids.filter((match) => match.referenceId !== referenceId);
    });
  }

  public clearMatches(): void {
    this.matchReferenceIds.set([]);
  }
}
