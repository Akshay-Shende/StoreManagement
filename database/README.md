EF Core migrations are intentionally generated from the model rather than hand-maintained in this scaffold.

Run:

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef migrations add InitialCreate --project src/StoreManagement.Infrastructure --startup-project src/StoreManagement.Api --output-dir Data/Migrations
dotnet ef database update --project src/StoreManagement.Infrastructure --startup-project src/StoreManagement.Api
```

Commit the generated `Data/Migrations` folder to source control after reviewing the SQL produced by EF Core.
