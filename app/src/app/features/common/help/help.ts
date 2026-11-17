import { DialogRef } from '@angular/cdk/dialog';
import { Component, Signal } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { ButtonGroup } from 'primeng/buttongroup';
import { environment } from '../../../../../environments/environment';
import { User } from '../../../core/models/domain/access/user';
import { CheckInOutService } from '../../../core/services/check-in-out.service';
import { UserService } from '../../../core/services/user.service';
import { Globals } from '../../../globals';
import { UpdateService } from '../../updater/update.service';
import { DialogHeader } from '../dialog-header/dialog-header';

@Component({
  selector: 'app-help',
  imports: [DialogHeader, ButtonDirective, ButtonGroup],
  templateUrl: './help.html',
  styleUrl: './help.scss',
  host: { class: 'dialog card' },
})
export class Help {
  constructor(
    private readonly _dialogRef: DialogRef,
    private readonly _updateService: UpdateService,
    private readonly _userService: UserService,
    private readonly _checkInOutService: CheckInOutService,
  ) {}

  public close(): void {
    this._dialogRef.close();
  }

  public checkForUpdates(): void {
    this._updateService.checkForUpdate();
  }

  public viewDocumentation(): void {
    const url = environment.documentationUrl;
    window.open(url, '_blank');
  }

  public get user(): Signal<User | undefined> {
    return this._userService.user;
  }

  public get version(): string {
    return Globals.AppVersion;
  }

  public get connectionId(): string {
    return this._checkInOutService.currentConnectionId;
  }
}
