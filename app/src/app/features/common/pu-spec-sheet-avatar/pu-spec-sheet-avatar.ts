import { Component, computed, inject, input } from '@angular/core';
import { CheckInOutService } from '../../../core/services/check-in-out.service';
import { Avatar } from '../avatar/avatar';
import { AvatarSize } from '../avatar/avatar-size.type';

/** The people who have a sheet open. */
@Component({
  selector: 'app-pu-spec-sheet-avatar',
  imports: [Avatar],
  templateUrl: './pu-spec-sheet-avatar.html',
  styleUrl: './pu-spec-sheet-avatar.scss',
})
export class PUSpecSheetAvatar {
  public readonly puSpecSheetId = input.required<number>();
  public readonly size = input<AvatarSize>('medium');
  public readonly viewers = computed(() => {
    const presences = this._checkInOutService.presences();
    const users = presences.get(this.puSpecSheetId()) ?? [];

    // One avatar per person, however many tabs they have open.
    return users.filter(
      (user, index) => users.findIndex((u) => u.uid === user.uid) === index,
    );
  });

  private readonly _checkInOutService = inject(CheckInOutService);
}
