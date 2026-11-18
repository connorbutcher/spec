import { computed, inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { RouteParamUtil } from '../../../core/helpers/route-param.util';
import { PhasesService } from '../../../core/services/phases.service';
import { TypesService } from '../../../core/services/types.service';
import { SheetsDataService } from './sheets-data.service';

@Injectable()
export class SheetsService {
  public readonly specSheetsForPhaseAndType = computed(
    () => this._sheetsForPhaseAndTypeResource.value() ?? [],
  );
  public readonly isLoading = computed(() =>
    this._sheetsForPhaseAndTypeResource.isLoading(),
  );
  public readonly latestSpecSheet = computed(
    () => this.specSheetsForPhaseAndType()[0] ?? null,
  );
  public readonly selectedSpecSheet = computed(() => {
    this._navigationEnd();

    const version =
      this._router.routerState.snapshot.root.queryParamMap.get('version');
    if (version === null) {
      return this.latestSpecSheet();
    }

    return (
      this.specSheetsForPhaseAndType().find(
        (sheet) => sheet.versionNumber === Number(version),
      ) ?? null
    );
  });
  public readonly selectedTypeId = computed<number | null>(() => {
    this._navigationEnd();

    const typeId = RouteParamUtil.findRouteParam(
      this._router.routerState.snapshot.root,
      'sheetTypeId',
    );
    if (typeId === null || isNaN(Number(typeId))) {
      return null;
    }

    return Number(typeId);
  });
  public readonly selectedType = computed(() => {
    const typeId = this.selectedTypeId();
    const selectedPhase = this._phasesService.selectedPhase();
    if (typeId === null || selectedPhase === null) {
      return null;
    }

    const typeExists = selectedPhase.types.some((type) => type.id === typeId);
    if (!typeExists) {
      return null;
    }

    return (
      this._typesService.specSheetTypes().find((type) => type.id === typeId) ??
      null
    );
  });
  public readonly isSelectedSheetLatest = computed(() => {
    const selectedSheet = this.selectedSpecSheet();
    const latestSheet = this.latestSpecSheet();
    if (selectedSheet === null || latestSheet === null) {
      return false;
    }

    return selectedSheet.versionNumber === latestSheet.versionNumber;
  });

  private readonly _sheetsDataService = inject(SheetsDataService);
  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _sheetsForPhaseAndTypeResource =
    this._sheetsDataService.getSpecSheetsForPhaseAndTypeResource(
      this._phasesService.selectedPhaseId,
      this.selectedType,
    );

  private readonly _navigationEnd = toSignal(
    this._router.events.pipe(filter((event) => event instanceof NavigationEnd)),
  );

  public reload(): void {
    this._sheetsForPhaseAndTypeResource.reload();
  }

  public selectType(typeId: number | null): void {
    this._router.navigate(['sheets', typeId], {
      relativeTo: this._activatedRoute,
      queryParams: { version: null },
      queryParamsHandling: 'merge',
    });
  }

  public selectVersion(versionNumber: number | null): void {
    this._router.navigate([], {
      queryParams: {
        version:
          versionNumber === this.latestSpecSheet()?.versionNumber
            ? null
            : versionNumber,
      },
      queryParamsHandling: 'merge',
      preserveFragment: true,
    });
  }
}
