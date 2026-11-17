import { computed, signal, WritableSignal } from '@angular/core';
import { SpecSheetTypeStructureLevel } from '../../../core/models/domain/configuration/spec-sheet-type-structure-level';
import { CreateSpecSheetTypeStructureLevelRequest } from '../../../core/models/requests/types/structure/create-spec-sheet-type-structure-level-request';
import { ManageSpecSheetTypeStructureLevelsRequest } from '../../../core/models/requests/types/structure/manage-spec-sheet-type-structure-level-value-request';
import { UpdateSpecSheetTypeStructureLevelRequest } from '../../../core/models/requests/types/structure/update-spec-sheet-type-structure-level-request';
import { StructureLevelType } from '../../../core/types/configuration/structure-level-type.enum';
import { StructureLevel } from './structure-level';

export class StructureLevels {
  public readonly levels: WritableSignal<StructureLevel[]>;

  public readonly isValid = computed(
    () =>
      this.levels().every((level) => level.isValid()) &&
      this.levels().length > 0 &&
      !this.hasDuplicatePropertiesAcrossGroups(),
  );
  public readonly hasChanges = computed(() => {
    const originalLevels = this.structureLevels.map((l) => l.id);
    const currentLevels = this.levels()
      .filter((l) => !l.isNew)
      .map((l) => l.id);
    const deletedLevels = originalLevels.filter(
      (id) => !currentLevels.some((l) => l === id),
    );

    return (
      this.levels().some((level) => level.hasChanges()) ||
      deletedLevels.length > 0
    );
  });
  public readonly hasDuplicatePropertiesAcrossGroups = computed(() => {
    const groups = this.levels().filter(
      (level) => level.levelType() === StructureLevelType.Group,
    );
    const allProperties = groups.flatMap((group) =>
      group.levelValues().map((value) => value.propertyReferenceId()),
    );
    const uniqueProperties = new Set(allProperties);
    return uniqueProperties.size !== allProperties.length;
  });

  constructor(public readonly structureLevels: SpecSheetTypeStructureLevel[]) {
    this.levels = signal(
      structureLevels.map(
        (structureLevel) => new StructureLevel(structureLevel),
      ),
    );
  }

  public addLevel(level: SpecSheetTypeStructureLevel): void {
    this.levels.update((current) => [...current, new StructureLevel(level)]);
  }

  public removeLevel(level: StructureLevel): void {
    this.levels.update((current) => current.filter((l) => l !== level));
  }

  public getNewLevels(): CreateSpecSheetTypeStructureLevelRequest[] {
    return this.levels()
      .filter((level) => level.isNew)
      .map((level) => level.toNewStructureLevel());
  }

  public getUpdatedLevels(): UpdateSpecSheetTypeStructureLevelRequest[] {
    return this.levels()
      .filter((level) => !level.isNew && level.hasChanges())
      .map((level) => level.toUpdateStructureLevelRequest());
  }

  public toManageStructureLevelsRequest(): ManageSpecSheetTypeStructureLevelsRequest {
    return {
      createLevelRequests: this.getNewLevels(),
      updateLevelRequests: this.levels()
        .filter((level) => !level.isNew && level.hasChanges())
        .map((level) => level.toUpdateStructureLevelRequest()),
      deleteLevelRequests: this.getDeletedLevelIds(),
    };
  }

  private getDeletedLevelIds(): number[] {
    const originalLevelValueIds = this.structureLevels.flatMap((level) =>
      level.levelValues.map((value) => value.id),
    );
    const currentLevelValueIds = this.levels()
      .flatMap((level) => level.levelValues())
      .filter((value) => !value.isNew)
      .map((value) => value.id);
    const deletedLevelValueIds = originalLevelValueIds.filter(
      (id) => !currentLevelValueIds.some((currentId) => currentId === id),
    );
    return deletedLevelValueIds;
  }
}
