import { HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SheetDataService } from '../../features/sheet/services/sheet-data.service';

export const legacySpecSheetGuard: CanActivateFn = async (route) => {
  const router = inject(Router);
  const sheetDataService = inject(SheetDataService);
  const id = Number(route.paramMap.get('id'));

  if (!Number.isSafeInteger(id) || id <= 0) {
    return router.createUrlTree(['/phases']);
  }

  try {
    const sheet = await sheetDataService.getSpecSheetByIdAsync(id);
    return router.createUrlTree(
      ['/phases', sheet.specSheetPhaseId, 'sheets', sheet.specSheetTypeId],
      {
        queryParams: { ...route.queryParams, version: id },
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
