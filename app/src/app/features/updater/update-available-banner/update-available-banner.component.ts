import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Observable } from 'rxjs';
import { NgSwAppData } from '../update-available-dialog/data/ngsw-app-data';
import { UpdateService } from '../update.service';

@Component({
  selector: 'app-update-available-banner',
  imports: [AsyncPipe, ButtonDirective],
  templateUrl: './update-available-banner.component.html',
  styleUrl: './update-available-banner.component.scss',
})
export class UpdateAvailableBannerComponent {
  public readonly hasDeferredUpdateAvailable: Observable<
    NgSwAppData | undefined
  >;

  constructor(private readonly _updateService: UpdateService) {
    this.hasDeferredUpdateAvailable =
      this._updateService.deferredInstallStatusChange$;
  }

  public installUpdate() {
    this._updateService.activateUpdateAsync();
  }
}
