import { PublishScopeOption } from './models/publish-scope-option.model';
import { PublishScope } from './models/publish-scope';
import { SheetDraftSummary } from './models/sheet-draft-summary.model';

/** How many changes other people have waiting on the sheet. */
export function otherDraftCount(others: readonly SheetDraftSummary[]): number {
  return others.reduce((total, summary) => total + summary.draftCount, 0);
}

/** "3 by A. Smith and 1 by B. Jones": whose changes publishing everything would take, besides the viewer's. */
export function otherDraftsLabel(others: readonly SheetDraftSummary[]): string {
  const parts = others.map((summary) => `${summary.draftCount} by ${summary.userName}`);
  if (parts.length <= 1) {
    return parts.join('');
  }
  return `${parts.slice(0, -1).join(', ')} and ${parts[parts.length - 1]}`;
}

/**
 * The two things a publish can take, each with its count. A choice with nothing in it can't be chosen:
 * the viewer's own when they have changed nothing, everything when nobody else has.
 */
export function publishScopeOptions(
  myCount: number,
  others: readonly SheetDraftSummary[],
): PublishScopeOption[] {
  const otherCount = otherDraftCount(others);
  return [
    {
      scope: 'Mine',
      label: 'Only my changes',
      count: myCount,
      detail: myCount === 0 ? 'You have no changes waiting.' : null,
      disabled: myCount === 0,
    },
    {
      scope: 'All',
      label: 'All pending changes',
      count: myCount + otherCount,
      detail:
        otherCount === 0
          ? 'Nobody else has changes waiting.'
          : `Includes ${otherDraftsLabel(others)}. Each stays in its author's name, and their rows are unlocked.`,
      disabled: otherCount === 0,
    },
  ];
}

/**
 * The choice in force: what the user picked if it can still be chosen, otherwise their own changes, and
 * everything only when they have none of their own.
 */
export function effectivePublishScope(
  options: readonly PublishScopeOption[],
  chosen: PublishScope | null,
): PublishScope {
  const usable = options.filter((option) => !option.disabled).map((option) => option.scope);
  if (chosen !== null && usable.includes(chosen)) {
    return chosen;
  }
  return usable.includes('Mine') || usable.length === 0 ? 'Mine' : 'All';
}

/** "Publish (3)": the button's words, with how many changes it will send out when there are any. */
export function publishLabel(count: number): string {
  return count > 0 ? `Publish (${count})` : 'Publish';
}
