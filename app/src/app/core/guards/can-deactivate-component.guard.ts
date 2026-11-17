import { Dialog } from '@angular/cdk/dialog';
import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CanDeactivate } from '../../features/common/can-deactivate/can-deactivate';
import { ConfirmationDialog } from '../../features/common/confirmation-dialog/confirmation-dialog';
import { ConfirmationDialogData } from '../../features/common/confirmation-dialog/models/confirmation-dialog-data.interface';
import { SaveChangesDialog } from '../../features/common/save-changes-dialog/save-changes-dialog';
import { DeactivationType } from '../types/deactivation-type.enum';

export const canDeactivateComponentGuard: CanDeactivateFn<
  CanDeactivate
> = async (component, _, __, next) => {
  const dialog = inject(Dialog);

  const skipCheck = next.root.queryParamMap.get('skipCheck');
  if (skipCheck && skipCheck.toLocaleLowerCase() === 'true') {
    return true;
  }

  try {
    if (component.hasChanges()) {
      //Spawn confirmation dialog, has changes needs to consider whether they have it actively checked out.
      const saveChangesResult = await firstValueFrom(
        dialog.open<DeactivationType, unknown, SaveChangesDialog>(
          SaveChangesDialog,
        ).closed,
      );
      if (
        saveChangesResult === undefined ||
        saveChangesResult === DeactivationType.Cancel
      ) {
        return false;
      }

      await component.handleExitAsync(saveChangesResult);
      return true;
    } else {
      await component.handleExitAsync(DeactivationType.Exit);
      return true;
    }
  } catch (e) {
    const result = await firstValueFrom(
      dialog.open<boolean, ConfirmationDialogData, ConfirmationDialog>(
        ConfirmationDialog,
        {
          data: {
            title: 'Invalid State',
            message:
              'The pu spec sheet is in an invalid state and cannot be saved. Please fix the errors before leaving.',
            confirmText: 'I will fix',
            cancelText: 'Close and lose changes',
          },
        },
      ).closed,
    );

    if (!result) {
      await component.handleExitAsync(DeactivationType.Exit);
      return true;
    }

    return false;
  }
};
