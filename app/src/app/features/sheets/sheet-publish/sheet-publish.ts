import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { TextareaModule } from 'primeng/textarea';
import { DialogActions } from '../../../shared/components/dialog-actions/dialog-actions';
import { ApprovalsStore } from '../../approvals/approvals.store';
import { PendingChangesUtil } from '../actions/pending-changes.util';
import { SheetAnchorService } from '../anchors/sheet-anchor.service';
import { SheetPublishApprovers } from '../sheet-publish-approvers/sheet-publish-approvers';
import { SheetPublishChanges } from '../sheet-publish-changes/sheet-publish-changes';
import { PublishScopeOption } from '../models/publish-scope-option.model';
import { PublishScope } from '../models/publish-scope';
import { PublishScopeUtil } from '../actions/publish-scope.util';
import { SheetPublishScope } from '../sheet-publish-scope/sheet-publish-scope';
import { SheetStore } from '../sheet.store';

/**
 * The Publish button: submits changes on the sheet for publishing as its next version. In a popover it
 * asks whose changes to send (the user's own, which is the default, or everything waiting on the sheet),
 * who must approve them, and for an optional note, and lists the changes, each a link to it on the sheet.
 * Nothing is published until every chosen approver approves; until then the changes are frozen.
 */
@Component({
  selector: 'app-sheet-publish',
  imports: [
    ButtonModule,
    DialogActions,
    FormsModule,
    PopoverModule,
    SheetPublishApprovers,
    SheetPublishChanges,
    SheetPublishScope,
    TextareaModule,
  ],
  templateUrl: './sheet-publish.html',
  styleUrl: './sheet-publish.scss',
})

export class SheetPublish {
  public readonly note = signal('');

  /** The people chosen to approve; kept between openings until a submission goes through. */
  public readonly approverIds = signal<number[]>([]);

  public readonly approvals = inject(ApprovalsStore);

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

  public readonly confirmLabel = computed(() => PublishScopeUtil.submitLabel(this.countOf(this.scope())));

  public readonly hasChanges = computed(() => this.countOf('All') > 0);

  public readonly hasApprovers = computed(() => this.approverIds().length > 0);

  public readonly isBusy = computed(() => this.store.isBusy());

  /** Each table, section and row the chosen publish would take, to check before sending it out. */
  public readonly changes = computed(() =>
    PendingChangesUtil.pendingChanges(this.store.sheet(), this.scope()),
  );

  private readonly store = inject(SheetStore);
  private readonly anchors = inject(SheetAnchorService);
  private readonly chosenScope = signal<PublishScope | null>(null);

  public open(event: Event, popover: Popover): void {
    this.approvals.loadApprovers();
    popover.toggle(event);
  }

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
    if (await this.store.publish(note === '' ? null : note, this.scope(), this.approverIds())) {
      this.note.set('');
      this.chosenScope.set(null);
      popover.hide();
      this.approvals.reload();
    }
  }

  private countOf(scope: PublishScope): number {
    return this.options().find((option) => option.scope === scope)?.count ?? 0;
  }
}
