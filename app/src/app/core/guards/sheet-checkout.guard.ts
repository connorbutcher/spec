import { Dialog } from '@angular/cdk/dialog';
import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { RequestingCheckoutDialog } from '../../features/common/requesting-checkout-dialog/requesting-checkout-dialog';
import { SheetCheckedOutDialogData } from '../../features/common/sheet-checked-out-dialog/models/sheet-checked-out-dialog-data';
import { SheetCheckedOutDialog } from '../../features/common/sheet-checked-out-dialog/sheet-checked-out-dialog';
import { CheckoutRequest } from '../models/check-in-out/checkout-request';
import { CheckInOutService } from '../services/check-in-out.service';

export const sheetCheckoutGuard: CanActivateFn = async (route, state) => {
  const checkInOutService = inject(CheckInOutService);
  const dialog = inject(Dialog);

  const routeId = route.paramMap.get('id')
    ? Number(route.paramMap.get('id'))
    : null;
  if (!routeId) {
    return false;
  }

  const isCheckedOut = checkInOutService.isPUSpecSheetCheckedOut(routeId);
  if (isCheckedOut) {
    const confirmRef = dialog.open<
      boolean | undefined,
      SheetCheckedOutDialogData,
      SheetCheckedOutDialog
    >(SheetCheckedOutDialog, {
      data: {
        title: 'Spec Sheet Checked Out',
        message:
          'This spec sheet is checked out. You can continue opening in read-only format, or request the checkout from them',
      },
    });

    const result = await firstValueFrom(confirmRef.closed);
    if (result === undefined) {
      return false;
    }

    if (result) {
      const request =
        await checkInOutService.invokeRequestCheckoutAsync(routeId);
      if (!request) {
        return false;
      }

      const dialogRef = dialog.open<
        boolean,
        CheckoutRequest,
        RequestingCheckoutDialog
      >(RequestingCheckoutDialog, {
        data: request,
      });
      const dialogResult = await firstValueFrom(dialogRef.closed);
      if (!dialogResult) {
        return false;
      }

      return true;
    }
  }

  return true;
};
