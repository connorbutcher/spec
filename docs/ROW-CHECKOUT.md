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

Auth is off locally, so every request is Local Admin unless it says otherwise:

- The API's `DefaultUserClaimsAccessor` reads an `X-Local-User` header (`1`, `2` or `3`) and acts as Local Admin, Local
  Engineer or Local Approver (`LocalUsers`). No header means Local Admin, as before.
- `database/seed-local-users.sql` gives Local Engineer and Local Approver the admin's permissions and makes all three
  approvers of every phase (data only, safe to rerun):
  `sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheetLegacy -E -b -I -i database/seed-local-users.sql`
- The UI has an "Act as" select in the side nav (development builds only, expand the nav to change it). It is kept per
  browser tab (sessionStorage), so two tabs can be two users at once. It is sent as the header and is the SignalR user
  name, so presence shows the right people.

## Commits

1. `030db52` Plan.
2. `93e638d` API: locks enforced in the draft helpers; 409 with the reason (`SheetEditExceptionFilter`).
3. `03e9e92` API: undo (all mine, one table, group or row).
4. `00eeee1` API: submit only my drafts, several pending submissions per sheet, per-submission decide, withdraw and
   reassign; a published SpecSheets row made at publish; create only for a new phase and type.
5. `d68b97c` API: local users for testing.
6. `1124dc8` Check-in/out service: presence and change notices instead of the whole-sheet checkout.
7. `a11cb08` UI: presence client, act-as switch, models; old checkout guard and dialogs removed.
8. `1311af6` UI: editing in place with lock tags, read-only rows, toolbar (version, checkouts, Undo My Changes,
   Submit/Publish), row and table undo, per-submission approvals.
9. `6e70b6e` A failed email no longer fails a committed submit or decision; seed-local-users.sql kept out of the schema.

## What was checked (2026-11-18)

- API, check-in/out service and UI all build (`dotnet build`, `ng build`).
- The API was run on a scratch database (deploy.ps1, seed.ps1, seed-local-users.sql) and driven as the three local users:
  create refused for an existing sheet; undo of everything on a never-published sheet removes it; Engineer refused
  (409, naming Local Admin) on the Admin's checked-out cell, on adding a row inside the Admin's new group and on removing
  the Admin's table; submit refused with nothing checked out and with yourself as approver; submitted items refused for
  everyone, the author included; reject hands them back; resubmit and approve publishes v1; a non-approver's decision is
  refused; after v1 the Engineer checks out the row and the Admin is refused it; the Admin renames the table (a separate
  lock); both submit; approving only the Admin's publishes v2 with the rename and without the Engineer's value; the
  Engineer withdraws, then undoes, and the row shows v2 again; v1 still reads as it was.
- Not checked in a browser: the UI against the new API (lock tags, read-only cells, toolbar, presence, change reloads,
  approvals panel). That needs the API and check-in/out service restarted on this branch.

## Known limits

- A row or group removed in someone's draft is hidden from everyone's view of the sheet until it is submitted or undone,
  so others don't see who is removing it. Undo all brings it back for the remover.
- Table notes and widgets aren't greyed out in the UI when someone else has the table checked out; the API refuses the
  save with the holder's name.
- Reordering rows touches every row moved, so it is refused when someone else has one of them checked out.
- Approve/Reject in the toolbar acts on the first pending submission the user hasn't decided; the approvals side panel
  has buttons for each submission.
- The sheet types' existing data: drafts left from the old "Create New Version" flow belong to whoever made them and are
  now their checkouts; an old unpublished SpecSheets header row from that flow is left alone and no longer used.