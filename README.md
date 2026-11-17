# PU Spec Sheet

Engine specification sheets, built from table templates and versioned by phase and sheet type.

## Layout

```
api/                                .NET solution (PUSpecSheet.slnx)
  src/PUSpecSheet.Api                 ASP.NET Core host: controllers and startup
  src/PUSpecSheet.CheckInOut.Api      SignalR host: checking sheet tables in and out
  src/PUSpecSheet.Api.Common          Authentication, authorisation, CORS, logging, Redis and Swagger set-up shared by both hosts
  src/PUSpecSheet.Api.DAL             Data access services, the change processors and the query resolvers
  src/PUSpecSheet.Api.Database        EF Core DbContext, schema classes, their configurations and mappers
  src/PUSpecSheet.Api.Emails          Notification emails
  src/PUSpecSheet.Abstractions        Domain models, requests and operations shared with the UI
  src/PUSpecSheet.Query               The query models and requests
app/                                Angular UI
database/                           SQL database project: the schema, one script per object, and the SqlPackage scripts
docs/legacy-schema.txt              The database's tables and columns
```

## Running

Fill in the connection string, Azure AD and Redis settings in `appsettings.Development.json` for both hosts, then:

```bash
dotnet run --project api/src/PUSpecSheet.Api
dotnet run --project api/src/PUSpecSheet.CheckInOut.Api
```

```bash
cd app && npm install && npm start
```

## Bringing in the new app

This codebase is the base. The rewrite it replaces, with revision-based versioning, approvals and the published API, is in commit `2c82183` (the commit before this one), and is ported onto this codebase in stages:

1. Make this codebase the repo's base. *(done)*
2. Put the database schema under source control in a SQL database project. *(done; generated from the DbContext and the schema dump, not yet compared with a live database)*
3. Add the revision tables in their own schemas, beside the existing ones.
4. Bring across the migrator, re-runnable, and keep the revision tables in step with the existing ones.
5. Serve the query API and read-only screens from the revision tables, checked against the existing answers first.
6. Move editing onto drafts and revisions one sheet type at a time, behind a per-type switch.
7. Port the remaining features one at a time: template versioning, cell kinds, the published API, lookups, presence.
8. Retire the copied-sheet tables and the code that writes them.
