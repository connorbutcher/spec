import {
  holderLabel,
  itemStatus,
  stateText,
  statusText,
  statusTone,
  versionText,
} from './item-status.util';
import { SheetChange } from './models/sheet-change.model';
import { SheetLock } from './models/sheet-lock.model';
import { StatusSource } from './models/status-source.model';

const V7: SheetChange = { versionNumber: 7, atUtc: '2026-09-12T10:00:00Z', userName: 'A. Smith' };
const MINE: SheetLock = { userId: 1, userName: 'Me Myself', isMine: true };
const THEIRS: SheetLock = { userId: 2, userName: 'A. Smith', isMine: false };

function source(lock: SheetLock | null, isPending = false): StatusSource {
  return { lock, isPending, lastChange: isPending ? null : V7 };
}

describe('itemStatus', () => {
  it('is Published on its version when nobody holds the item', () => {
    const status = itemStatus(source(null), null);
    expect(status.state).toBe('Published');
    expect(statusText(status)).toBe('v7 · Published');
    expect(stateText(status)).toBeNull();
    expect(statusTone(status)).toBeNull();
  });

  it('is Checked out when someone is only in the row', () => {
    const status = itemStatus(source({ ...THEIRS, isCheckout: true }), null);
    expect(status.state).toBe('CheckedOut');
    expect(statusText(status)).toBe('v7 · Checked out · A. Smith');
    expect(statusTone(status)).toBe('theirs');
  });

  it('is a Draft when a change is saved, and says You for the viewer', () => {
    const status = itemStatus(source(MINE), null);
    expect(status.state).toBe('Draft');
    expect(holderLabel(status)).toBe('You');
    expect(statusText(status)).toBe('v7 · Draft · You');
    expect(statusTone(status)).toBe('mine');
  });

  it('is New, with no version, until it is first published', () => {
    const status = itemStatus(source(MINE, true), null);
    expect(versionText(status)).toBe('New');
    expect(statusText(status)).toBe('New · Draft · You');
  });

  it('has no version text for an item that keeps none', () => {
    const status = itemStatus({ lock: THEIRS, isPending: false }, null);
    expect(versionText(status)).toBeNull();
    expect(statusText(status)).toBe('Draft · A. Smith');
  });

  it('is Publishing while a publish of my changes runs, for my drafts only', () => {
    expect(itemStatus(source(MINE), 'Mine').state).toBe('Publishing');
    expect(itemStatus(source(THEIRS), 'Mine').state).toBe('Draft');
  });

  it('is Publishing for every draft while a publish of everything runs', () => {
    const status = itemStatus(source(THEIRS), 'All');
    expect(status.state).toBe('Publishing');
    expect(stateText(status)).toBe('Publishing');
    expect(statusTone(status)).toBe('publishing');
  });

  it('never publishes a row someone is only in', () => {
    expect(itemStatus(source({ ...THEIRS, isCheckout: true }), 'All').state).toBe('CheckedOut');
  });

  it('gives an item waiting for approval its own colour', () => {
    const status = { ...itemStatus(source(THEIRS), null), state: 'ForApproval' as const };
    expect(stateText(status)).toBe('For approval · A. Smith');
    expect(statusTone(status)).toBe('approval');
  });
});
