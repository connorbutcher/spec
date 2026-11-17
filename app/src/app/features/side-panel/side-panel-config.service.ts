import { Injectable, signal } from '@angular/core';

@Injectable()
export class SidePanelConfigService {
  public readonly panelWidth = signal<number>(500);
  public readonly canNavigate = signal<boolean>(true);
  public readonly canClose = signal<boolean>(true);
}
