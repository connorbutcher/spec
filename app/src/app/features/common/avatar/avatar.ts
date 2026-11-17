import { NgClass } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import {
  Component,
  computed,
  effect,
  input,
  resource,
  signal,
} from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { lastValueFrom } from 'rxjs';
import { AvatarSize } from './avatar-size.type';
import { AvatarPipe } from './avatar.pipe';

@Component({
  selector: 'app-avatar',
  imports: [AvatarPipe, NgClass, Tooltip],
  templateUrl: './avatar.html',
  styleUrl: './avatar.scss',
})
export class Avatar {
  public readonly avatar = input.required<string>();
  public readonly uid = input.required<string>();
  public readonly backgroundColor = input.required<string>();
  public readonly index = input.required<number>();
  public readonly size = input<AvatarSize>('medium');

  public readonly imageUrl = computed<string | undefined>(() => {
    const profilePicture = this.profilePictureResource.value();
    if (profilePicture) {
      return URL.createObjectURL(profilePicture);
    }

    return undefined;
  });
  public readonly hasImage = signal<boolean>(true);
  public readonly isHovering = signal<boolean>(false);

  private readonly profilePictureResource = resource({
    params: () => ({
      uid: this.uid(),
    }),
    loader: ({ params }) =>
      lastValueFrom(
        this._httpClient.get(
          `https://graph.microsoft.com/v1.0/users/${params.uid}/photo/$value`,
          { responseType: 'blob' },
        ),
      ),
  });

  constructor(private readonly _httpClient: HttpClient) {
    effect(() => {
      if (this.profilePictureResource.error()) {
        this.hasImage.set(false);
      }
    });
  }

  public setHovering(isHovering: boolean): void {
    this.isHovering.set(isHovering);
  }
}
