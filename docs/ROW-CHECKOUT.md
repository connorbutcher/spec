# Row and table checkout (replaces "Create New Version")

Branch `row-checkout`. Local only, nothing pushed. One small commit per step below.

## What changes for users

- There is no "Create New Version" any more. The latest view of a sheet is always open for editing.
- Editing a row or table (or group, or a sheet property) checks it out to you. It stays checked out to you after you
  save, close the browser or come back tomorrow, until you either **undo** your changes or **submit** them.
- Everyone sees who has each row or table checked out (a small name tag on the row and table header), and the sheet's
  current version (for example "v4, 3 rows checked out").
- Only you can change what you have checked out. Others can still edit every other row.
- **Undo** throws away your checked-out changes (all of them, or one row/table) and frees them.
- **Submit** sends your checked-out changes for approval. While they wait, nobody can edit them (you included).
  - Approved: they are published as the sheet's next version and the rows are free to edit again.
  - Rejected: they come back to you, still checked out to you, to fix and resubmit or undo.
  - Sheet types that don't need approval publish your changes as the next version straight away.
- Several people can have changes waiting at once. Each approval publishes the next version with only that person's
  changes.
- Old versions (`?version=N`) stay read-only. Creating the first sheet for a phase and type ("Create Spec Sheet") stays.

## How it works (no schema change)

The revision tables already have everything needed:

- **The lock is the draft.** An item (table, group, row, property) has at most one draft revision (filtered unique index
  `UX_..._OneDraft`). Its `CreatedByUserID` is who has it checked out. Its `SheetSubmissionID` says it is waiting for
  approval.
- **Edits** (`PATCH /sheets/{id}/draft`): the draft helpers (`TableDrafts`, `GroupDrafts`, `RowDrafts`,
  `SheetPropertyDrafts`) refuse a draft that belongs to someone else or is in a submission, with a 409 naming the holder.
  Adding a row or group inside an item that only exists as someone else's draft is refused too, so a submission never
  depends on someone else's.
- **Undo**: `DELETE /sheets/{id}/draft/mine` drops your open drafts (what you added goes with them);
  `DELETE /sheets/{id}/draft/mine/{kind}/{itemId}` drops one.
- **Submit**: `PATCH /sheets/{id}/publish` tags your open drafts with a new `SheetSubmission`. Approval (or a type
  without approval) publishes only the drafts of that submission as the next version. Decisions and reassignments name
  the submission: `PATCH /sheets/{id}/submissions/{submissionId}/decision|approvers`.
- **The SpecSheets header** is no longer created up front. A published version still gets its `SpecSheets` row (the
  query API, cache and copy-from use `SourceSpecSheetId`), made at publish time from the previous version's properties.
  Workflow steps on the header are no longer written; submissions are the record.
- `SheetItemRevision` gains `SheetSubmissionId` so the UI can tell "checked out" from "awaiting approval".
- `POST /sheets/draft` only creates a sheet that doesn't exist yet (the first version). The drafts it makes are the
  creator's.

## Check-in/out service (SignalR, port 8889)

The whole-sheet checkout (one connection holds the sheet, request/override dialogs) goes: locks now live in the
database and survive disconnects. The hub becomes presence and change notices:

- `JoinSheetAsync(sheetId)` / `LeaveSheetAsync(sheetId)`: who is viewing a sheet (`SheetPresence` to the sheet's group).
- `SheetChangedAsync(sheetId)`: called by the UI after a save, undo, submit or decision; the others viewing the sheet get
  `SheetChanged(sheetId, user)` and reload (straight away if they have nothing unsaved, otherwise with a toast).
- Connected users and their colours stay as they are.

## UI

- Toolbar: "Create New Version" removed. Shows the version and checkout summary; buttons Save, Undo my changes, Submit
  (Publish for types without approval) enabled from your own checkouts.
- Rows and table headers show a lock tag with the holder's name (and "awaiting approval"); cells of rows locked to others
  or awaiting approval are read-only. Row context menu gets "Undo my changes to this row".
- Approvals: the approval status and side panel list each pending submission separately, with approve/reject per
  submission.
- `sheetCheckoutGuard`, the requesting/requested checkout dialogs and the whole-sheet checkout in `CheckInOutService` are
  removed.

## Testing with more than one user

Auth is off locally, so every request is Local Admin. For testing:

- The API's `DefaultUserClaimsAccessor` reads an `X-Local-User` header (`1`, `2` or `3`) and acts as Local Admin, Local
  Engineer or Local Approver (`DefaultUsers`). No header means Local Admin, as now.
- `database/seed-local-users.sql` adds the two extra users with edit and approve permissions (data only, safe to rerun).
- The UI has a small "Act as" select in the top bar (development builds only) stored in localStorage, sent as the header
  and used as the SignalR user name. Use a normal and a private browser window to be two users at once.

## Steps (commits)

1. This plan.
2. API: `SheetSubmissionId` on `SheetItemRevision`; locks enforced in the draft helpers; 409 for lock conflicts.
3. API: undo (all mine, one item).
4. API: submit/publish only my drafts; per-submission decide and reassign; header made at publish; create only for new
   sheets.
5. API: local users for testing (header, seed script).
6. Check-in/out service: presence and change notices instead of whole-sheet checkout.
7. UI: models, services and the act-as switch.
8. UI: toolbar (no Create New Version; version, save, undo, submit).
9. UI: lock tags and read-only rows, row undo.
10. UI: approvals per submission; remove the old checkout guard and dialogs.
11. Docs: PROGRESS.md and this file brought up to date with what was checked.
