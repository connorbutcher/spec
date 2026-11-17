import { computed, signal, WritableSignal } from '@angular/core';
import { SpecSheetTypeStructureLevelValue } from '../../../core/models/domain/configuration/spec-sheet-type-structure-level-value';
import { CreateSpecSheetTypeStructureLevelValueRequest } from '../../../core/models/requests/types/structure/create-spec-sheet-type-structure-level-value-request';
import { UpdateSpecSheetTypeStructureLevelValueRequest } from '../../../core/models/requests/types/structure/update-spec-sheet-type-structure-level-value-request';

export class StructureLevelValue {
  public readonly id: number;
  public readonly specSheetTypeStructureLevelId: number;
  public readonly displayOrder: WritableSignal<number>;
  public readonly propertyReferenceId: WritableSignal<string>;

  public readonly isValid = computed(
    () =>
      this.displayOrder() > 0 && this.propertyReferenceId().trim().length > 0,
  );
  public readonly hasChanges = computed(
    () =>
      this.displayOrder() !== this.structureLevelValue.displayOrder ||
      this.propertyReferenceId() !==
        this.structureLevelValue.specSheetTypePropertyReferenceId ||
      this.isNew,
  );

  constructor(
    public readonly structureLevelValue: SpecSheetTypeStructureLevelValue,
  ) {
    this.id = structureLevelValue.id;
    this.specSheetTypeStructureLevelId =
      structureLevelValue.specSheetTypeStructureLevelId;
    this.displayOrder = signal(structureLevelValue.displayOrder);
    this.propertyReferenceId = signal(
      structureLevelValue.specSheetTypePropertyReferenceId,
    );
  }

  public toModel(): SpecSheetTypeStructureLevelValue {
    return {
      id: this.id,
      specSheetTypeStructureLevelId: this.specSheetTypeStructureLevelId,
      displayOrder: this.displayOrder(),
      specSheetTypePropertyReferenceId: this.propertyReferenceId(),
    };
  }

  public getNewProperty(): CreateSpecSheetTypeStructureLevelValueRequest {
    return {
      specSheetTypeStructureLevelId: this.specSheetTypeStructureLevelId,
      displayOrder: this.displayOrder(),
      specSheetTypePropertyReferenceId: this.propertyReferenceId(),
    };
  }

  public getUpdatedProperty(): UpdateSpecSheetTypeStructureLevelValueRequest {
    return {
      id: this.id,
      specSheetTypePropertyReferenceId: this.propertyReferenceId(),
      displayOrder: this.displayOrder(),
    };
  }

  public get isNew(): boolean {
    return this.id <= 0;
  }
}
