# Store Management POC - Local Debug Guide

This repository contains the production-hardening work requested for local development and debugging. It is intentionally suitable for a developer workstation and is **not** a claim that every deployment concern has been externally verified.

## Backend prerequisites

- .NET SDK 10.x
- SQL Server / SQL Server Express / LocalDB
- EF Core CLI (`dotnet tool install --global dotnet-ef` if needed)

## First database setup

The domain model changed substantially (goods receipts, batch allocations, payments, returns, audit, notifications, concurrency fields). The migration files in older revisions are therefore not the authoritative schema for this branch.

On a disposable local database, generate a fresh migration:

```powershell
dotnet ef migrations add ProductionHardening `
  --project src/StoreManagement.Infrastructure `
  --startup-project src/StoreManagement.Api `
  --output-dir Migrations

dotnet ef database update `
  --project src/StoreManagement.Infrastructure `
  --startup-project src/StoreManagement.Api
```

If your local database already contains the old POC schema, use a separate test database or drop/recreate the database before applying the new migration.

## Configuration

Development defaults are in `src/StoreManagement.Api/appsettings.Development.json`.

For another SQL Server instance, override:

```text
ConnectionStrings__StoreDb
JwtSettings__Secret
JwtSettings__Issuer
JwtSettings__Audience
```

The refresh token is now stored in an HttpOnly cookie by the API. The Angular application keeps the access token in memory.

## Run backend

```powershell
dotnet restore

dotnet build StoreManagement.sln
dotnet run --project src/StoreManagement.Api
```

Swagger is configured at the application root to avoid accidentally creating `/swagger` as a separate IIS application. The old IIS 500.35 issue should therefore not be reproduced by this configuration when the IIS site itself is configured as one ASP.NET Core application.

## Run frontend

From the Angular repository:

```powershell
npm ci
npm start
```

The Angular proxy sends `/api/*` requests to `http://localhost:5240`.

## Local test account

The development database seed contains the demo admin account used by the POC. Verify the exact credentials in the development seed before use rather than copying credentials into production.

## High-value scenarios to debug

1. Create purchase -> receive full quantity -> verify product stock, batch stock and inventory ledger.
2. Create purchase for 100 -> receive 60 -> verify `PartiallyReceived` and remaining 40 -> receive 40 -> verify `Received`.
3. Receive the same purchase item into two batches -> verify both batch allocations are preserved.
4. Try receiving more than remaining quantity -> expect `409` and no stock/ledger mutation.
5. Sell a product across two batches -> verify FEFO and `SaleItemBatch` allocations.
6. Attempt to sell expired stock -> request must fail.
7. Create a sale -> add one or more payments -> verify paid/due and completion status.
8. Cancel an unpaid/reserved sale -> verify stock is restored exactly once.
9. Return a sold good-condition item -> verify stock is restored to the original batch.
10. Return a damaged item -> verify it is not added to sellable stock.
11. Create and approve a stock adjustment -> verify one ledger entry and updated stock.
12. Submit the same idempotency key twice -> verify one logical operation.
13. Send two simultaneous sales against the last stock -> verify no negative stock.
14. Expire a batch -> verify it disappears from sellable stock and gets a ledger entry.
15. Disable a user -> verify refresh fails and protected API actions are rejected.
16. Expire an access token -> verify one refresh attempt and original request retry.
17. Break refresh-token validity -> verify requests do not hang and the client returns to login.
18. Use cashier role to attempt manager-only purchase/adjustment actions -> expect `403`.
19. Compare product stock, batch stock and ledger totals using the reconciliation endpoint.
20. Create report queries around midnight in `Asia/Kolkata` and verify business-day boundaries.

## Important local limitations

- Regenerate the EF migration before using a fresh database.
- Run the real .NET build/test suite locally because this workspace does not include the .NET SDK.
- Run `npm ci`, `npm run build`, and `npm test` locally because dependency installation is environment-dependent.
- Complete infrastructure-level backup/restore, monitoring and deployment testing in your own environment.
