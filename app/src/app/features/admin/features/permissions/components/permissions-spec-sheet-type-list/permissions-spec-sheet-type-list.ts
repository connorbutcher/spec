import { Component, effect, inject, model, Signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Listbox, ListboxChangeEvent } from 'primeng/listbox';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { TypesService } from '../../../../../../core/services/types.service';
import { PermissionsService } from '../../services/permissions.service';

@Component({
  selector: 'app-permissions-spec-sheet-type-list',
  imports: [Listbox, FormsModule],
  templateUrl: './permissions-spec-sheet-type-list.html',
  styleUrl: './permissions-spec-sheet-type-list.scss',
})
export class PermissionsSpecSheetTypeListComponent {
  public readonly selectedSpecSheetType = model<SpecSheetType | undefined>(
    undefined,
  );

  private readonly _permissionsService = inject(PermissionsService);
  private readonly _typesService = inject(TypesService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  constructor() {
    effect(() => {
      const activeMode = this._permissionsService.activeMode();
      if (activeMode === 'bytype') {
        const specSheetType = this._typesService
          .specSheetTypes()
          .find((sst) => sst.id === this._permissionsService.activeId());
        if (specSheetType) {
          this.selectedSpecSheetType.set(specSheetType);
        }
      } else {
        this.selectedSpecSheetType.set(undefined);
      }
    });
  }

  public onChange(event: ListboxChangeEvent) {
    if (!event.value) {
      return;
    }

    const specSheetType = event.value as SpecSheetType;
    if (!specSheetType) {
      return;
    }

    this._router.navigate(['type', specSheetType.id], {
      relativeTo: this._activatedRoute,
    });
  }

  public get specSheetTypes(): Signal<SpecSheetType[]> {
    return this._typesService.specSheetTypes;
  }
}
