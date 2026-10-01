import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { TextareaModule } from 'primeng/textarea';
import { SheetStore } from '../sheet.store';

/**
 * The Publish button: publishes everything the user has changed on the sheet as its next version. It
 * asks for an optional note first, in a popover, and shows how many changes will go out.
 */
@Component({
  selector: 'app-sheet-publish',
  imports: [ButtonModule, FormsModule, PopoverModule, TextareaModule],
  templateUrl: './sheet-publish.html',
  styleUrl: './sheet-publish.scss',
})
export class SheetPublish {
  public readonly note = signal('');

  public readonly changeCount = computed(() => this.store.sheet()?.myDraftCount ?? 0);

  public readonly label = computed(() =>
    this.changeCount() > 0 ? `Publish (${this.changeCount()})` : 'Publish',
  );

  public readonly isBusy = computed(() => this.store.isBusy());

  private readonly store = inject(SheetStore);

  public async publish(popover: Popover): Promise<void> {
    const note = this.note().trim();
    if (await this.store.publish(note === '' ? null : note)) {
      this.note.set('');
      popover.hide();
    }
  }
}
