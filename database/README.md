# PUSpecSheet.Database

The database's schema, kept as a SQL database project (Microsoft.Build.Sql): one script per table, key, index and constraint, under `<schema>/<object type>/`. Building it checks every object and reference and produces a dacpac, which SqlPackage compares with or deploys to a database.

The API reads and writes the database through EF Core (`api/src/PUSpecSheet.Api.Database`) but doesn't own the schema: there are no EF migrations. This project is the record of what the schema is. Change it here, deploy it, then update the EF schema classes and configurations to match.

## Scripts

All three restore SqlPackage as a local dotnet tool and take `-Server` and `-Database` (Windows authentication, defaulting to `(localdb)\MSSQLLocalDB` and `PUSpecSheet`) or `-ConnectionString` for anything else.

| Script | What it does |
| --- | --- |
| `extract.ps1` | Writes a database's schema into the project, replacing the schema folders. Review the result with git before committing it. |
| `compare.ps1` | Builds the project and lists what deploying it would change in a database, objects only in the database included. Changes nothing. Exits with 1 when there are differences. |
| `deploy.ps1` | Builds the project and makes a database match it, creating the database if needed. Stops instead of losing data. |

Database options, users, logins and permissions are kept out of the project and left alone by all three, since they differ between environments.

## First extract

The project starts empty. Fill it from the production schema (or a copy of it):

```powershell
./database/extract.ps1 -ConnectionString 'Server=...;Database=...;...'
dotnet build database
./database/compare.ps1 -ConnectionString 'Server=...;Database=...;...'   # expect no differences
```

then commit the scripts. From then on, `compare.ps1` against each environment shows where it has drifted from the project.

## Why not generate it from the DbContext

`dotnet ef dbcontext script` can write a create script from the EF model, but the model only holds what the API needs to map: the configurations have no indexes, few delete rules and EF's own guesses at constraint and default names. A schema built from it would differ from the real database, and deploying it would try to drop what's missing. Extracting from the database records what's actually there. The DbContext script is still a useful cross-check that the EF mapping agrees with the project.
