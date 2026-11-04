import {
  ItemStatusUtil } from './item-status.util';
import { SheetChange } from './models/sheet-change.model';
import { SheetLock } from './models/sheet-lock.model';
import { StatusSource } from './models/status-source.model';

class SpecHelpers {
  public static source(lock: SheetLock | null, isPending = false): StatusSource {
    return { lock, isPending, lastChange: isPending ? null : V7 };
  }
}

const V7: SheetChange = { versionNumber: 7, atUtc: '2026-09-12T10:00:00Z', userName: 'A. Smith' };
const MINE: SheetLock = { userId: 1, userName: 'Me Myself', isMine: true };
const THEIRS: SheetLock = { userId: 2, userName: 'A. Smith', isMine: false };
describe('itemStatus', () => {
  it('is Published on its version when nobody holds the item', () => {
    const status = ItemStatusUtil.itemStatus(SpecHelpers.source(null), null);
    expect(status.state).toBe('Published');
    expect(ItemStatusUtil.statusText(status)).toBe('v7 · Published');
    expect(ItemStatusUtil.stateText(status)).toBeNull();
    expect(ItemStatusUtil.statusTone(status)).toBeNull();
  });

  it('is Checked out when someone is only in the row', () => {
    const status = ItemStatusUtil.itemStatus(SpecHelpers.source({ ...THEIRS, isCheckout: true }), null);
    expect(status.state).toBe('CheckedOut');
    expect(ItemStatusUtil.statusText(status)).toBe('v7 · Checked out · A. Smith');
    expect(ItemStatusUtil.statusTone(status)).toBe('theirs');
  });

  it('is a Draft when a change is saved, and says You for the viewer', () => {
    const status = ItemStatusUtil.itemStatus(SpecHelpers.source(MINE), null);
    expect(status.state).toBe('Draft');
    expect(ItemStatusUtil.holderLabel(status)).toBe('You');
    expect(ItemStatusUtil.statusText(status)).toBe('v7 · Draft · You');
    expect(ItemStatusUtil.statusTone(status)).toBe('mine');
  });

  it('is New, with no version, until it is first published', () => {
    const status = ItemStatusUtil.itemStatus(SpecHelpers.source(MINE, true), null);
    expect(ItemStatusUtil.versionText(status)).toBe('New');
    expect(ItemStatusUtil.statusText(status)).toBe('New · Draft · You');
  });

  it('has no version text for an item that keeps none', () => {
    const status = ItemStatusUtil.itemStatus({ lock: THEIRS, isPending: false }, null);
    expect(ItemStatusUtil.versionText(status)).toBeNull();
    expect(ItemStatusUtil.statusText(status)).toBe('Draft · A. Smith');
  });

  it('is Publishing while a publish of my changes runs, for my drafts only', () => {
    expect(ItemStatusUtil.itemStatus(SpecHelpers.source(MINE), 'Mine').state).toBe('Publishing');
    expect(ItemStatusUtil.itemStatus(SpecHelpers.source(THEIRS), 'Mine').state).toBe('Draft');
  });

  it('is Publishing for every draft while a publish of everything runs', () => {
    const status = ItemStatusUtil.itemStatus(SpecHelpers.source(THEIRS), 'All');
    expect(status.state).toBe('Publishing');
    expect(ItemStatusUtil.stateText(status)).toBe('Publishing');
    expect(ItemStatusUtil.statusTone(status)).toBe('publishing');
  });

  it('never publishes a row someone is only in', () => {
    expect(ItemStatusUtil.itemStatus(SpecHelpers.source({ ...THEIRS, isCheckout: true }), 'All').state).toBe('CheckedOut');
  });

  it('gives an item waiting for approval its own colour', () => {
    const status = { ...ItemStatusUtil.itemStatus(SpecHelpers.source(THEIRS), null), state: 'ForApproval' as const };
    expect(ItemStatusUtil.stateText(status)).toBe('For approval · A. Smith');
    expect(ItemStatusUtil.statusTone(status)).toBe('approval');
  });
});
