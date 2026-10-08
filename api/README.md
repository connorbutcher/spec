# PU Spec Sheet API

How the API is put together and how to change it. Running it, the linters and the API documentation are covered in the [root README](../README.md); the schema is described in [database/README.md](../database/README.md).

## Projects

```
src/PUSpecSheet.Api           ASP.NET Core host. Controllers, authorization, exception handling, OpenAPI, compression, CORS.
src/PUSpecSheet.Application   Use cases. One folder per feature; services, the queries they run and the rules they apply.
src/PUSpecSheet.Contracts     Request and response records. The only types that cross the HTTP boundary.
src/PUSpecSheet.Data          EF Core: the DbContext, one configuration class per entity, migrations, seed data.
src/PUSpecSheet.Domain        Entities, enums and rules that need nothing but themselves.
tests/PUSpecSheet.*.Tests     One test project per source project.
```

References go one way: `Api -> Application -> Data -> Domain`. `Contracts` is used by `Api` and `Application` and references `Domain` only for enums and the cell configuration records. Nothing references `Api`.

| Folder in `Application` | What it holds |
| --- | --- |
| `Sheets` | Reading a sheet, editing it through drafts, publishing. The largest feature; see below. |
| `Published` | The read-only API other applications call: whole sheets, rows by id, lookups by a keyed value. |
| `Templates` | Table templates and their versions, sections, rows, cells and column blocks. |
| `CellTypes`, `Phases`, `SheetTypes`, `Users` | The reference data and who the current user is. |
| `Common` | The three application exceptions, display ordering and the sized cache. |

## A request, end to end

1. **Controller** (`Api/Controllers`). Binds the route and body, calls one service method and returns its result. No rules, no `try`/`catch`, no EF. `[ApiController]` turns a request that fails its data annotations into a 400 before the action runs.
2. **Service** (`Application/<Feature>`). Loads what it needs through `PuSpecSheetDbContext`, applies the rules, saves, and returns a contract record. Anything wrong is thrown as `NotFoundException`, `InvalidRequestException` or `ConflictException`.
3. **`ApplicationExceptionHandler`** turns those three into 404, 400 and 409 ProblemDetails. `PermissionDeniedResultHandler` does the same for a missing permission (403). Everything else is a 500.
4. **Mapping** to contracts happens in the service or a `...Mappings` class. Entities never leave `Application`.

Every method that touches the database is `async` and takes the request's `CancellationToken` as its last parameter.

## Sheets: revisions, drafts and versions

A sheet item (table, section, row, column block) is an identity row plus revisions. Three ideas explain most of the code:

- **A draft is a lock.** Changing an item starts a draft revision owned by the user. The database allows one draft per item, so nobody else can change it until the author publishes or discards. `DraftGateway<TRevision>` (one subclass per item kind) loads an item's current revision and draft and starts drafts.
- **Publishing makes a version.** `SheetPublisher` turns drafts into published revisions under a new `SheetVersion`, and closes the revisions they replace at the same instant. A published revision is never edited again. The request's `PublishScope` says whose drafts: the publisher's own (`Mine`, the default) or every draft on the sheet (`All`). A draft published for someone else keeps its author, so the change stays in their name while the version is in the publisher's, and their lock goes with it; a draft of theirs that changes nothing is dropped, not published.
- **Any moment can be read back.** A revision is in force from `PublishedAtUtc` until `SupersededAtUtc`, so "version 3" or "as of last Tuesday" is a range check.

Reading goes `SheetReader` -> `SheetSnapshotLoader` (the queries) -> `SheetViewBuilder` (the tree the screen shows). Every edit returns the refreshed sheet through the same path.

Two things to keep when adding queries here:

- Ask for live revisions with `Current()`, `CurrentAndDrafts()` or `VisibleTo(userId)` from `SheetRevisionQueryExtensions`, not `SupersededAtUtc == null`. They match the filtered unique indexes; the bare predicate reads every revision the items have ever had.
- Load many items in one query. `DraftGateway.LoadManyAsync`, `ReleaseUnchangedAsync` and `RowValueStore.SetManyAsync` exist so nothing loops over items issuing a query each.

Cell values live in the `values` schema, one table per kind (text, numeric, date, boolean, option), keyed by row revision and cell. `RowValueStore` is the only class that reads or writes them for the editing screens.

Settings chosen for a cell on the sheet (`CellInstanceSettings`, such as the table and column a linked dropdown reads) live beside the values in `values.CellSettings`, with the same key, and travel with them through `RowValueStore` and `CellValueBag`. That is all it takes for them to follow every draft, publish and history rule above. `SheetCellSettingsService` saves them, and `RowDraftStarter` is the one place a row's draft is started, for values and settings alike. How the linked dropdown works, and how to add another setting or give another kind settings, is in the [root README](../README.md#cell-settings-chosen-on-the-sheet).

## Multi-user editing

Several people can have a sheet open and edit it at once. A row belongs to someone in one of two ways:

- **They have changed it.** That is its draft revision in the database, as before: it stays theirs until they publish or discard, and it shows as `row.lock` in the `SheetDto`.
- **They are in it but have not changed it.** Clicking into a cell must not save anything, so this is a *live checkout*, held in memory against their browser tab's connection (`LiveRowCheckoutTracker`). It ends when they leave the row, close the tab or lose the connection. It is not in the `SheetDto`; it is sent over the hub.

`Sheets/Collaboration` and the SignalR hub in `Api/Collaboration` add four things:

- **Presence.** A browser keeps one connection to `SheetHub` (`/hubs/sheets`) and calls `JoinSheet` for the sheet it has open. `SheetPresenceTracker` holds connections by sheet in memory and everyone on the sheet is told when the list changes.
- **Live checkouts.** The browser calls `CheckOutRow` on the hub when its user clicks into a row and `ReleaseRow` when they leave (`LiveRowCheckoutService`); everyone on the sheet gets the new list (`CheckoutsChanged`). A tab holds one row at a time. `LiveRowCheckoutGuard` is asked wherever a row is about to be changed (`SheetRowService`), so someone else can't save into a row another person is in.
- **Live changes.** Every action that changes a sheet already answers with the refreshed `SheetDto`. `SheetChangedFilter` looks at that answer and, when something other people can see has changed (who a row is checked out to, or the latest version; see `SheetLockSignature`), tells the sheet's group to read it again. The hub sends no sheet data, because each person sees a different view. A new editing endpoint needs nothing extra as long as it returns `SheetDto`.
- **Takeovers.** Someone can ask for a row that is checked out to another person (`RowTakeoversController`). The holder approves or denies; a request nobody answers within `RowTakeover:ResponseSeconds` (60) is granted, and it is granted straight away when the holder doesn't have the sheet open. Only a row its holder has **not changed** can be taken over: once a row has unpublished changes it stays with whoever made them until they publish or discard, so a takeover never moves anyone's work. Waiting requests are in memory (`RowTakeoverStore`): they last a minute and only matter to connected people.

The takeover code is four small classes, so each rule has one place:

| Class | Its one job |
| --- | --- |
| `RowTakeoverService` | What people do: ask, approve, deny, withdraw. Checks who is allowed to. Every reason a request is refused is in `CheckoutToAskForAsync`, which also answers `GET .../takeover-availability`, so the screen and the request can't disagree. |
| `RowTakeoverSettler` | What nobody does: grants requests that ran out of time (called by `RowTakeoverExpiryWorker`) and closes ones that can no longer succeed because the row was released or changed (called by `SheetChangedFilter`). |
| `RowTakeoverCloser` | Ends a request, whichever way it ends: hands the row over if it still can be (`ObstacleAsync` says why not), and tells the two people. |
| `RowCheckouts` (`IRowCheckouts`) | Who holds a row, of either kind, behind one question: the draft in the database (and whether it differs from what is published) or the live checkout. Also moves a row from one person to another. `RowTakeoverTests` swaps it for a fake, so the rules are tested without a database. |

| Rule | Where it lives |
| --- | --- |
| Clicking into a row checks it out without saving; leaving, closing the tab or dropping the connection frees it | `LiveRowCheckoutService`, `SheetPresenceService.LeaveAsync` |
| A row someone else is in, or has changed, can't be entered or changed | `LiveRowCheckoutService.CheckOutAsync`, `LiveRowCheckoutGuard` |
| A changed row can't be asked for | `RowTakeoverService.CheckoutToAskForAsync` |
| A takeover of a row its holder is only in moves it to the requester's tab | `RowCheckouts.TransferLiveAsync` |
| A row changed while a request waits stays with its holder (`KeptForChanges`), whether the holder approves, the time runs out, or neither | `RowTakeoverCloser.ObstacleAsync` |
| What counts as changed: place, existence or any cell value differs from what is published | `RowCheckouts.HasChangesAsync`, the same comparison `RowDrafts` uses to drop a no-op draft |
| Only the holder answers, only the requester withdraws | `RowTakeoverService.Take` |
| One waiting request per row; asking twice is the same request | `RowTakeoverStore.TryAdd`, `RowTakeoverService.RequestAsync` |
| Holder not on the sheet: granted at once. No answer in time: granted | `RowTakeoverService.RequestAsync`, `RowTakeoverSettler.GrantOverdueAsync` |

A request is always taken out of `RowTakeoverStore` before it is settled; that is what stops an answer and the timeout both settling it. To add a way for a request to end, add a `RowTakeoverStatus`, call `RowTakeoverCloser` from the service or the settler, and add its wording to `takeover-notice.util.ts` in the UI.

The hub takes who a connection is from its signed-in user (`ClaimsPrincipalExtensions.FindUserId`), and `ICurrentUser` reads the same claim, so replacing the developer sign-in with real authentication changes neither. All of this state is per process: running more than one API instance would need a SignalR backplane and a shared store for presence and waiting requests.

To try it before sign-in exists, Development seeds two more users (`engineer2`, `engineer3`) and lets a request name the user it runs as (`DeveloperSignIn:AllowUserSwitching`): open the app in a second tab with `?developerUser=engineer2`.

## Published API

`Published` reads only published revisions, by public identifier (the GUIDs), never by database id. What a version held never changes, so answers are cached in memory (`PublishedSheetCache`, `PublishedRowsCache`) and served with ETags; `PublishedSheetHttpCache` holds the HTTP side. Its queries project straight to small records (`Published...Record`) and the `...Assembler` classes, which are pure, build the response. The field names and routes of this API are a contract with other applications: change them only on purpose.

## How to

**Add an endpoint to an existing feature**

1. Add the request/response records to `Contracts/<Feature>`, one per file, with data annotations for required fields, lengths and ranges.
2. Add the method to the service interface and implement it.
3. Add the action to the controller with XML comments (`<summary>`, `<remarks>`, `<param>`, `<response>` for each status it can return). 400, 403, 404 and 409 are documented where they apply by `ProblemResponsesConvention`; add `[ProducesResponseType]` only for anything else.
4. Decide who may call it. An action that changes anything takes `[Authorize(Policy = PermissionKeys.X)]` (on the controller when every action shares it); `EndpointPermissionTests` fails if one is missing. A read needs nothing more: the fallback policy already requires a signed-in user. Only the published API is `[AllowAnonymous]`. A new permission is a constant in `PermissionKeys` (add it to `All`) and a seeded row in `PermissionConfiguration`, which needs a migration.

**Add a feature**: a folder in `Application` with `I<Feature>Service` and its implementation, registered in `ApplicationServiceCollectionExtensions` (or the feature's own `Add...` extension, as `Sheets` and `Published` do); a controller with `[Tags(ApiTags.X)]`, with the tag added to `ApiTags` and `ApiTagCatalog`.

**Add an entity**

1. The class in `Domain/<Area>`.
2. An `IEntityTypeConfiguration<T>` in `Data/Configurations`: table name, every string length, every foreign key with an explicit `OnDelete`, indexes for the ways it will be queried, and check constraints for rules the database can hold.
3. A `DbSet` on `PuSpecSheetDbContext`.
4. A migration, below.

**Add a migration**

```bash
dotnet ef migrations add AddSomething --project src/PUSpecSheet.Data --startup-project src/PUSpecSheet.Api --configuration Release
```

Name it for what it does. Read the generated file before committing: one migration per change, nothing unrelated in it. The API applies migrations on start in Development. Then bring the database project back in step and commit its scripts with the migration:

```powershell
./database/refresh.ps1
./database/compare.ps1
```

`dotnet ef migrations has-pending-model-changes` (same project arguments) confirms the snapshot matches the model.

## Conventions

The build enforces the mechanical ones and fails on any analyzer warning: braces always, file-scoped namespaces, explicit accessibility, one type per file named after it. The rest are by review:

- Services, records and helpers are `sealed` (or `static`) unless designed for inheritance; entities are plain classes for EF. Dependencies come in through primary constructors.
- A service does one job. When a method repeats itself for tables, sections, rows and column blocks, the shared part belongs in a generic helper constrained to `ISheetRevision` (see `SheetPublisher.SupersedeAsync`).
- Reads use `AsNoTracking()` and project to what they need. Writes that span statements use a transaction and `SaveSheetChangesAsync`, which turns a concurrency clash into a 409.
- Text from a request is trimmed, and blank optional text is stored as null.
- XML comments say why, or what a caller must know. They don't restate the name.

## Tests

```bash
dotnet test PUSpecSheet.slnx
```

Tests run without a database. Three kinds:

- **Rules as pure functions.** Logic is kept out of the classes that query (`SheetChangeHistoryCalculator`, `SheetCellPlanner`, `OrderGaps`, `PhaseMover`, the published assemblers) so it can be tested with plain objects. New logic should follow the same split.
- **SQL shape.** `PublishedQueryTranslationTests` and `SheetRevisionQueryTranslationTests` build a context with no server and assert on `ToQueryString()`, which catches a query that stops translating, pulls extra columns or stops matching an index.
- **Model.** `PUSpecSheet.Data.Tests` checks the EF model itself (public ids, seeded permissions).

What they don't cover is anything that needs real SQL Server behaviour: the publish transaction, draft locking under concurrency, cascade deletes. Those have been checked by replaying a scripted scenario against a scratch copy of the database and comparing every response before and after a change.
