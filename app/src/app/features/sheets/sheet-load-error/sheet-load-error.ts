import { Component, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';

/** Shown in place of the sheet when it, or the phases it belongs to, could not be loaded. */
@Component({
  selector: 'app-sheet-load-error',
  imports: [ButtonModule, EmptyState],
  templateUrl: './sheet-load-error.html',
  styleUrl: './sheet-load-error.scss',
})
export class SheetLoadError {
  public readonly retry = output<void>();
}
