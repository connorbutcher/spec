import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { TextareaModule } from 'primeng/textarea';
import { PublishScopeOption } from '../models/publish-scope-option.model';
import { PublishScope } from '../models/publish-scope';
import { effectivePublishScope, publishLabel, publishScopeOptions } from '../publish-scope.util';
import { SheetPublishScope } from '../sheet-publish-scope/sheet-publish-scope';
import { SheetStore } from '../sheet.store';

/**
 * The Publish button: publishes changes on the sheet as its next version. In a popover it asks whose
 * changes to send out (the user's own, which is the default, or everything waiting on the sheet) and for
 * an optional note, and shows how many changes will go out.
 */
@Component({
  selector: 'app-sheet-publish',
  imports: [ButtonModule, FormsModule, PopoverModule, SheetPublishScope, TextareaModule],
  templateUrl: './sheet-publish.html',
  styleUrl: './sheet-publish.scss',
})
export class SheetPublish {
  public readonly note = signal('');

  public readonly options = computed<PublishScopeOption[]>(() => {
    const sheet = this.store.sheet();
    return publishScopeOptions(sheet?.myDraftCount ?? 0, sheet?.otherDrafts ?? []);
  });

  /** What will be published: the user's pick while it can be chosen, otherwise their own changes. */
  public readonly scope = computed<PublishScope>(() =>
    effectivePublishScope(this.options(), this.chosenScope()),
  );

  /** The toolbar button counts the user's own changes, which is what it publishes unless they say otherwise. */
  public readonly label = computed(() => publishLabel(this.countOf('Mine')));

  public readonly confirmLabel = computed(() => publishLabel(this.countOf(this.scope())));

  public readonly hasChanges = computed(() => this.countOf('All') > 0);

  public readonly isBusy = computed(() => this.store.isBusy());

  private readonly store = inject(SheetStore);
  private readonly chosenScope = signal<PublishScope | null>(null);

  public chooseScope(scope: PublishScope): void {
    this.chosenScope.set(scope);
  }

  public async publish(popover: Popover): Promise<void> {
    const note = this.note().trim();
    if (await this.store.publish(note === '' ? null : note, this.scope())) {
      this.note.set('');
      this.chosenScope.set(null);
      popover.hide();
    }
  }

  private countOf(scope: PublishScope): number {
    return this.options().find((option) => option.scope === scope)?.count ?? 0;
  }
}
