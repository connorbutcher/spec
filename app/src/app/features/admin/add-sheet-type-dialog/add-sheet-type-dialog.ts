import { Component, computed, inject, model, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { SheetType } from '../../../core/models/sheet-type.model';
import { apiErrorMessage } from '../../templates/api-error-message';
import { AdminApi } from '../admin-api';

/** The dialog that adds a sheet type, which is just its name. */
@Component({
  selector: 'app-add-sheet-type-dialog',
  imports: [ButtonModule, DialogModule, FormsModule, InputTextModule, MessageModule],
  templateUrl: './add-sheet-type-dialog.html',
  styleUrl: './add-sheet-type-dialog.scss',
})
export class AddSheetTypeDialog {
  public readonly visible = model(false);

  public readonly created = output<SheetType>();

  public readonly name = signal('');
  public readonly error = signal<string | null>(null);
  public readonly isSaving = signal(false);

  public readonly canAdd = computed(() => this.name().trim() !== '' && !this.isSaving());

  private readonly api = inject(AdminApi);

  /** Clears the form each time the dialog opens. */
  public reset(): void {
    this.name.set('');
    this.error.set(null);
  }

  public async add(): Promise<void> {
    if (!this.canAdd()) {
      return;
    }

    this.isSaving.set(true);
    this.error.set(null);
    try {
      const sheetType = await this.api.createSheetType(this.name().trim());
      this.visible.set(false);
      this.created.emit(sheetType);
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.isSaving.set(false);
    }
  }
}
