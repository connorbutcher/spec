import { httpResource } from '@angular/common/http';
import { computed, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { SpecSheetType } from '../models/domain/types/spec-sheet-type';

@Injectable({
  providedIn: 'root',
})
export class TypesService {
  public readonly isLoading = computed(() =>
    this._specSheetTypesResource.isLoading(),
  );
  public readonly hasError = computed(() =>
    this._specSheetTypesResource.error(),
  );

  public readonly specSheetTypes = computed<SpecSheetType[]>(
    () => this._specSheetTypesResource.value() ?? [],
  );
  public readonly tableTemplates = computed(() =>
    this.specSheetTypes().flatMap((t) => t.tableTemplates),
  );
  public readonly tableCellTypes = computed(() => {
    const tableCellTypes = this.tableTemplates().flatMap((t) =>
      t.tableGroupTemplates.flatMap((gt) =>
        gt.rowTemplates.flatMap((w) =>
          w.cellTemplates.flatMap((ct) => ct.tableCellType),
        ),
      ),
    );

    return tableCellTypes.filter(
      (value, index, self) =>
        self.findIndex((tableCellType) => tableCellType.key === value.key) ===
        index,
    );
  });

  private readonly _specSheetTypesResource = httpResource<SpecSheetType[]>(
    () => `${environment.apiUrl}/specsheettypes/all`,
  );

  public refresh(): void {
    this._specSheetTypesResource.reload();
  }
}
