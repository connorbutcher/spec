import { HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SheetDataService } from '../../features/sheet/services/sheet-data.service';

/**
 * Sends a link to a sheet by its id (`spec/:id`, with an optional `?version=` version number) to the sheet's page.
 */
export const legacySpecSheetGuard: CanActivateFn = async (route) => {
  const router = inject(Router);
  const sheetDataService = inject(SheetDataService);
  const id = Number(route.paramMap.get('id'));

  if (!Number.isSafeInteger(id) || id <= 0) {
    return router.createUrlTree(['/phases']);
  }

  const version = route.queryParamMap.get('version');
  const versionNumber =
    version !== null && Number.isSafeInteger(Number(version))
      ? Number(version)
      : undefined;

  try {
    const detail = await sheetDataService.getPublishedSheetAsync(
      id,
      versionNumber,
    );
    return router.createUrlTree(
      [
        '/phases',
        detail.sheet.specSheetPhaseId,
        'sheets',
        detail.sheet.specSheetTypeId,
      ],
      {
        queryParams: {
          ...route.queryParams,
          version: versionNumber ?? null,
        },
        fragment: route.fragment ?? undefined,
      },
    );
  } catch (error) {
    if (error instanceof HttpErrorResponse && error.status === 404) {
      return router.createUrlTree(['/phases']);
    }
    throw error;
  }
};
