import { computed, signal, WritableSignal } from '@angular/core';
import { SpecSheetTypeStructureLevel } from '../../../core/models/domain/configuration/spec-sheet-type-structure-level';
import { CreateSpecSheetTypeStructureLevelRequest } from '../../../core/models/requests/types/structure/create-spec-sheet-type-structure-level-request';
import { UpdateSpecSheetTypeStructureLevelRequest } from '../../../core/models/requests/types/structure/update-spec-sheet-type-structure-level-request';
import { StructureLevelType } from '../../../core/types/configuration/structure-level-type.enum';
import { StructureLevelValue } from './structure-level-value';

export class StructureLevel {
  public readonly id: number;
  public readonly levelType: WritableSignal<StructureLevelType>;
  public readonly level: WritableSignal<number>;
  public readonly sortOrientation: WritableSignal<number>;
  public readonly levelValues: WritableSignal<StructureLevelValue[]>;

  public readonly isValid = computed(
    () => this.level() > 0 && this.levelValues().length > 0,
  );
  public readonly hasChanges = computed(() => {
    const originalValues = this.structureLevel.levelValues.map((v) => v.id);
    const currentValues = this.levelValues()
      .filter((v) => !v.isNew)
      .map((v) => v.id);
    const deletedValues = originalValues.filter(
      (id) => !currentValues.some((v) => v === id),
    );

    return (
      this.levelType() !== this.structureLevel.structureLevelType ||
      this.level() !== this.structureLevel.level ||
      this.sortOrientation() !== this.structureLevel.sortOrientation ||
      this.levelValues().some((value) => value.hasChanges()) ||
      deletedValues.length > 0 ||
      this.isNew
    );
  });

  constructor(public readonly structureLevel: SpecSheetTypeStructureLevel) {
    this.id = structureLevel.id;
    this.levelType = signal(structureLevel.structureLevelType);
    this.level = signal(structureLevel.level);
    this.sortOrientation = signal(structureLevel.sortOrientation);
    this.levelValues = signal(
      structureLevel.levelValues.map(
        (levelValue) => new StructureLevelValue(levelValue),
      ),
    );
  }

  public addLevelValue(levelValue: StructureLevelValue): void {
    this.levelValues.update((current) => [...current, levelValue]);
  }

  public toNewStructureLevel(): CreateSpecSheetTypeStructureLevelRequest {
    return {
      structureLevelType: this.levelType(),
      level: this.level(),
      sortOrientation: this.sortOrientation(),
      specSheetTypeId: this.structureLevel.specSheetTypeId,
      valueRequests: this.levelValues()
        .filter((value) => value.isNew)
        .map((value) => value.getNewProperty()),
    };
  }

  public toUpdateStructureLevelRequest(): UpdateSpecSheetTypeStructureLevelRequest {
    return {
      id: this.id,
      level: this.level(),
      sortOrientation: this.sortOrientation(),
      manageLevelValueRequest: {
        createLevelValueRequests: this.levelValues()
          .filter((value) => value.isNew)
          .map((value) => value.getNewProperty()),
        updateLevelValueRequests: this.levelValues()
          .filter((value) => !value.isNew && value.hasChanges())
          .map((value) => value.getUpdatedProperty()),
        deleteLevelValueRequests: this.getDeletedLevelValueIds(),
      },
    };
  }

  private getDeletedLevelValueIds(): number[] {
    const originalValues = this.structureLevel.levelValues.map((v) => v.id);
    const currentValues = this.levelValues()
      .filter((v) => !v.isNew)
      .map((v) => v.id);
    const deletedValues = originalValues.filter(
      (id) => !currentValues.some((v) => v === id),
    );
    return deletedValues;
  }

  public get isNew(): boolean {
    return this.id <= 0;
  }
}
