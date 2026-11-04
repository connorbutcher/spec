import { ItemState } from './models/item-state';
import { ItemStatus } from './models/item-status.model';
import { PublishScope } from './models/publish-scope';
import { StatusSource } from './models/status-source.model';
import { StatusTone } from './models/status-tone';
import { SheetLabelsUtil } from './sheet-labels.util';

const STATE_LABELS: Readonly<Record<ItemState, string>> = {
  Published: 'Published',
  CheckedOut: 'Checked out',
  Draft: 'Draft',
  ForApproval: 'For approval',
  Publishing: 'Publishing',
};

const STATE_ICONS: Readonly<Record<ItemState, string | null>> = {
  Published: null,
  CheckedOut: 'pi pi-circle',
  Draft: 'pi pi-pencil',
  ForApproval: 'pi pi-clock',
  Publishing: 'pi pi-upload',
};

export class ItemStatusUtil {
  /**
   * Where an item stands. Nobody holding it is `Published`; someone only in it is `CheckedOut`; a saved
   * change is a `Draft`, and `Publishing` for as long as the publish that takes it is running
   * (`publishing` is what that publish was asked to take, or null when none is running).
   */
  public static itemStatus(source: StatusSource, publishing: PublishScope | null): ItemStatus {
    const lock = source.lock;
    return {
      versionNumber: source.lastChange?.versionNumber ?? null,
      isNew: source.isPending,
      state: ItemStatusUtil.stateOf(source, publishing),
      userName: lock?.userName ?? null,
      isMine: lock?.isMine === true,
    };
  }

  /** "Draft": the state in words. */
  public static stateLabel(state: ItemState): string {
    return STATE_LABELS[state];
  }

  /** The icon class for a state, or null for `Published`, which is shown by its version alone. */
  public static stateIcon(state: ItemState): string | null {
    return STATE_ICONS[state];
  }

  /** "You" or the person's name: who has the item. Null when nobody does, or while it is being published. */
  public static holderLabel(status: ItemStatus): string | null {
    if (status.state === 'Published' || status.state === 'Publishing') {
      return null;
    }
    return status.isMine ? 'You' : status.userName;
  }

  /** "Draft · A. Smith": the state and who has it. Null for `Published`, which needs no words beside its version. */
  public static stateText(status: ItemStatus): string | null {
    if (status.state === 'Published') {
      return null;
    }
    const holder = ItemStatusUtil.holderLabel(status);
    const label = ItemStatusUtil.stateLabel(status.state);
    return holder === null ? label : `${label} · ${holder}`;
  }

  /** "v7" or "New": the version the item is on. Null for an item that doesn't keep one. */
  public static versionText(status: ItemStatus): string | null {
    if (status.isNew) {
      return 'New';
    }
    return status.versionNumber === null ? null : SheetLabelsUtil.versionLabel(status.versionNumber);
  }

  /** "v7 · Draft · A. Smith": the whole status in one line, as a screen reader hears it. */
  public static statusText(status: ItemStatus): string {
    return [ItemStatusUtil.versionText(status), ItemStatusUtil.stateText(status) ?? ItemStatusUtil.stateLabel(status.state)]
      .filter((part) => part !== null)
      .join(' · ');
  }

  /** The colour a state is shown in. Null for `Published`, which has none. */
  public static statusTone(status: ItemStatus): StatusTone | null {
    switch (status.state) {
      case 'Published':
        return null;
      case 'ForApproval':
        return 'approval';
      case 'Publishing':
        return 'publishing';
      default:
        return status.isMine ? 'mine' : 'theirs';
    }
  }

  private static stateOf(source: StatusSource, publishing: PublishScope | null): ItemState {
    const lock = source.lock;
    if (lock === null) {
      return 'Published';
    }
    if (lock.isCheckout === true) {
      return 'CheckedOut';
    }
    return publishing === 'All' || (publishing === 'Mine' && lock.isMine) ? 'Publishing' : 'Draft';
  }
}
