# PUSpecSheet.Database

The database's schema, kept as a SQL database project (Microsoft.Build.Sql): one script per table, with its keys, constraints and indexes, under `<schema>/Tables/`, and one per schema under `Security/`. Building it checks every object and reference and produces a dacpac, which SqlPackage compares with or deploys to a database.

The API reads and writes the database through EF Core (`api/src/PUSpecSheet.Api.Database`) but doesn't own the schema: there are no EF migrations. This project is the record of what the schema is. Change it here, deploy it, then update the EF schema classes and configurations to match.

## Scripts

All three restore SqlPackage as a local dotnet tool and take `-Server` and `-Database` (Windows authentication, defaulting to `(localdb)\MSSQLLocalDB` and `PUSpecSheet`) or `-ConnectionString` for anything else.

| Script | What it does |
| --- | --- |
| `extract.ps1` | Writes a database's schema into the project, replacing the schema folders. Review the result with git before committing it. |
| `compare.ps1` | Builds the project and lists what deploying it would change in a database, objects only in the database included. Changes nothing. Exits with 1 when there are differences. |
| `deploy.ps1` | Builds the project and makes a database match it, creating the database if needed. Stops instead of losing data. |

Database options, users, logins and permissions are kept out of the project and left alone by all three, since they differ between environments.

## Where the scripts came from

The scripts were written from the EF model (`PUSpecSheetDbContext` and its configurations), which decides the schemas, tables and columns, with the physical detail EF doesn't hold (varchar or nvarchar, lengths, nullability, defaults, column order and foreign key names) taken from the column dump of the database in `docs/legacy-schema.txt`. CellConfigurations isn't in the dump, so it comes from the EF model alone.

Two things are conventions rather than read from anywhere: primary keys are named `PK_<table>` and defaults `DF_<table>_<column>`, and every foreign key column has an index `IX_<table>_<column>` (unique for the one-to-one value tables), as EF would create. The database's own constraint and index names will differ, and `compare.ps1` will list them the first time it is run against it.

Foreign keys don't cascade, except SpecSheetTypePhases to SpecSheetTypes, the only cascade the EF model configures. EF would make every required relationship cascade, which SQL Server rejects here (a sheet table is reachable from a sheet type by more than one path).

### Where the EF model and the database disagreed

The EF configurations have since been brought into line with the database: varchar column types, the two columns that allow NULL (`WorkflowStepActors.ActorUserID`, `TableTemplates.RequiresUserConfiguration`), the foreign keys EF didn't know about (`TableGroupTemplates.ParentTableGroupTemplateID`, `LinkedSpecSheetValues.LinkedSpecSheetTypeID` and `LinkedSpecSheetPhaseID`), and SpecSheetLinkedTables, which is now mapped with the columns it has.

One difference is left on purpose: EF relates `SpecSheetTableNotes.CreatedByUserID` to Users so it can load the author, but the database has no foreign key for it. EF doesn't need one, and adding it could fail on notes whose author has gone.

The dump also gives every datetime2 column a scale of 0, so the scripts use `DATETIME2 (0)`. Confirm it against the database when you can; if it's really `DATETIME2 (7)`, `compare.ps1` will flag every one.

## Checking against a database

When someone can run the scripts against a database, run `compare.ps1` and settle what it lists, either by fixing the project or, where the database is right and the project wrong, with `extract.ps1`. Until then the project is the best reconstruction available, not a verified record.

## Why not just `dotnet ef dbcontext script`

It needs the EF design-time package and tools added to the API, and it only knows the EF model, so its output differs from the database everywhere in the table above. The scripts here start from the same model and correct it from the dump.
