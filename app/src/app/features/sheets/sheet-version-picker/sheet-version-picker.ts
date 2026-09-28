import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';

/**
 * Picks which version of the sheet to view. Disabled until sheet versions exist; it will default to
 * the latest version, with older versions read-only.
 */
@Component({
  selector: 'app-sheet-version-picker',
  imports: [FormsModule, SelectModule],
  templateUrl: './sheet-version-picker.html',
  styleUrl: './sheet-version-picker.scss',
})
export class SheetVersionPicker {
  public readonly versions = signal<string[]>([]);
  public readonly selected = signal<string | null>(null);
}
