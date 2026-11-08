import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { TextareaModule } from 'primeng/textarea';
import { DialogActions } from '../../../shared/components/dialog-actions/dialog-actions';
import { PendingChangesUtil } from '../actions/pending-changes.util';
import { SheetAnchorService } from '../anchors/sheet-anchor.service';
import { SheetPublishChanges } from '../sheet-publish-changes/sheet-publish-changes';
import { PublishScopeOption } from '../models/publish-scope-option.model';
import { PublishScope } from '../models/publish-scope';
import { PublishScopeUtil } from '../actions/publish-scope.util';
import { SheetPublishScope } from '../sheet-publish-scope/sheet-publish-scope';
import { SheetStore } from '../sheet.store';

/**
 * The Publish button: publishes changes on the sheet as its next version. In a popover it asks whose
 * changes to send out (the user's own, which is the default, or everything waiting on the sheet) and for
 * an optional note, and shows how many changes will go out and lists them, each a link to it on the sheet.
 */
@Component({
  selector: 'app-sheet-publish',
  imports: [
    ButtonModule,
    DialogActions,
    FormsModule,
    PopoverModule,
    SheetPublishChanges,
    SheetPublishScope,
    TextareaModule,
  ],
  templateUrl: './sheet-publish.html',
  styleUrl: './sheet-publish.scss',
})

export class SheetPublish {
  public readonly note = signal('');

  public readonly options = computed<PublishScopeOption[]>(() => {
    const sheet = this.store.sheet();
    return PublishScopeUtil.publishScopeOptions(sheet?.myDraftCount ?? 0, sheet?.otherDrafts ?? []);
  });

  /** What will be published: the user's pick while it can be chosen, otherwise their own changes. */
  public readonly scope = computed<PublishScope>(() =>
    PublishScopeUtil.effectivePublishScope(this.options(), this.chosenScope()),
  );

  /** The toolbar button counts the user's own changes, which is what it publishes unless they say otherwise. */
  public readonly label = computed(() => PublishScopeUtil.publishLabel(this.countOf('Mine')));

  public readonly confirmLabel = computed(() => PublishScopeUtil.publishLabel(this.countOf(this.scope())));

  public readonly hasChanges = computed(() => this.countOf('All') > 0);

  public readonly isBusy = computed(() => this.store.isBusy());

  /** Each table, section and row the chosen publish would take, to check before sending it out. */
  public readonly changes = computed(() =>
    PendingChangesUtil.pendingChanges(this.store.sheet(), this.scope()),
  );

  private readonly store = inject(SheetStore);
  private readonly anchors = inject(SheetAnchorService);
  private readonly chosenScope = signal<PublishScope | null>(null);

  public chooseScope(scope: PublishScope): void {
    this.chosenScope.set(scope);
  }

  /** Closes the popover and goes to the change on the sheet; the note typed so far is kept. */
  public showChange(key: string, popover: Popover): void {
    popover.hide();
    this.anchors.show(key);
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
