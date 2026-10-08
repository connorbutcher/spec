import { SheetPresenceUser } from './models/sheet-presence-user.model';
import { SheetRow } from './models/sheet-row.model';

/** The people on a sheet with the viewer moved to the front; everyone else keeps their order. */
export function viewerFirst(
  users: SheetPresenceUser[],
  viewerId: number | null,
): SheetPresenceUser[] {
  return [
    ...users.filter((user) => user.userId === viewerId),
    ...users.filter((user) => user.userId !== viewerId),
  ];
}

/** How many of the rows are checked out to each user, by user id. */
export function checkedOutRowCounts(rows: Iterable<SheetRow>): ReadonlyMap<number, number> {
  const counts = new Map<number, number>();
  for (const row of rows) {
    if (row.lock !== null) {
      counts.set(row.lock.userId, (counts.get(row.lock.userId) ?? 0) + 1);
    }
  }
  return counts;
}

/** "Engineer Two" → "ET": the first letters of the first two words of a name. */
export function initials(displayName: string): string {
  return displayName
    .split(/\s+/)
    .filter((word) => word !== '')
    .slice(0, 2)
    .map((word) => word[0].toUpperCase())
    .join('');
}

/** "Engineer Two (you) · 2 rows checked out · 3 tabs": one person on the sheet, spelled out. */
export function presenceDescription(
  user: SheetPresenceUser,
  isViewer: boolean,
  checkedOutRows: number,
): string {
  const parts = [isViewer ? `${user.displayName} (you)` : user.displayName];
  if (checkedOutRows > 0) {
    parts.push(`${checkedOutRows} ${checkedOutRows === 1 ? 'row' : 'rows'} checked out`);
  }
  if (user.connectionCount > 1) {
    parts.push(`${user.connectionCount} tabs`);
  }
  return parts.join(' · ');
}
