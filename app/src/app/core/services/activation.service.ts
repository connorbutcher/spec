import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ActivationService {
  public refToActivate = signal<string | undefined>(undefined);
}
