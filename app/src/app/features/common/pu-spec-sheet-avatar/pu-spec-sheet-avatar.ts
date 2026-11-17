import { Component, computed, inject, input } from '@angular/core';
import { CheckInOutService } from '../../../core/services/check-in-out.service';
import { Avatar } from '../avatar/avatar';
import { AvatarSize } from '../avatar/avatar-size.type';

@Component({
  selector: 'app-pu-spec-sheet-avatar',
  imports: [Avatar],
  templateUrl: './pu-spec-sheet-avatar.html',
  styleUrl: './pu-spec-sheet-avatar.scss',
})
export class PUSpecSheetAvatar {
  public readonly puSpecSheetId = input.required<number>();
  public readonly size = input<AvatarSize>('medium');
  public readonly checkoutForTemplate = computed(() => {
    const puSpecSheetCheckouts = this._checkInOutService.puSpecSheetCheckouts();
    const id = this.puSpecSheetId();
    if (!puSpecSheetCheckouts || puSpecSheetCheckouts.length === 0 || !id) {
      return undefined;
    }

    return puSpecSheetCheckouts.find((checkout) => checkout.id === id);
  });

  private readonly _checkInOutService = inject(CheckInOutService);
}
