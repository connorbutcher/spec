import { RowTakeover } from './models/row-takeover.model';
import { TakeoverNotice } from './models/takeover-notice.model';

/**
 * What to tell the viewer when a takeover request they are part of is settled, or null when they
 * already know (they answered it themselves) or it doesn't concern them.
 */
export function takeoverNotice(takeover: RowTakeover, viewerUserId: number): TakeoverNotice | null {
  if (takeover.requesterUserId === viewerUserId) {
    return requesterNotice(takeover);
  }
  return takeover.holderUserId === viewerUserId ? holderNotice(takeover) : null;
}

function requesterNotice(takeover: RowTakeover): TakeoverNotice | null {
  const holder = takeover.holderName;
  switch (takeover.status) {
    case 'Approved': {
      return granted(`${holder} handed the row over.`);
    }
    case 'GrantedOnTimeout': {
      return granted(`${holder} didn't answer in time.`);
    }
    case 'GrantedHolderAway': {
      return granted(`${holder} doesn't have the sheet open.`);
    }
    case 'Denied': {
      return {
        severity: 'warn',
        summary: 'Takeover refused',
        detail: `${holder} is keeping the row.`,
        sticky: false,
      };
    }
    case 'KeptForChanges': {
      return {
        severity: 'warn',
        summary: 'Takeover not possible',
        detail: `${holder} has changed the row, so it stays with them until they publish.`,
        sticky: false,
      };
    }
    case 'Released': {
      return {
        severity: 'info',
        summary: 'Row released',
        detail: `${holder} has finished with the row, so it is free to edit.`,
        sticky: false,
      };
    }
    default: {
      return null;
    }
  }
}

function holderNotice(takeover: RowTakeover): TakeoverNotice | null {
  const requester = takeover.requesterName;
  switch (takeover.status) {
    case 'GrantedOnTimeout': {
      return {
        severity: 'warn',
        summary: 'Row taken over',
        detail: `${requester} took over a row you had checked out, as the request went unanswered.`,
        sticky: true,
      };
    }
    case 'GrantedHolderAway': {
      return {
        severity: 'warn',
        summary: 'Row taken over',
        detail: `${requester} took over a row you had checked out.`,
        sticky: true,
      };
    }
    case 'Cancelled': {
      return {
        severity: 'info',
        summary: 'Request withdrawn',
        detail: `${requester} no longer needs the row.`,
        sticky: false,
      };
    }
    default: {
      return null;
    }
  }
}

function granted(reason: string): TakeoverNotice {
  return {
    severity: 'success',
    summary: 'Row checked out to you',
    detail: `${reason} The row is now yours to edit.`,
    sticky: false,
  };
}
