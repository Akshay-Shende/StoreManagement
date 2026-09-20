$ErrorActionPreference = "Stop"

Write-Host "== Backend restore/build =="
dotnet restore
dotnet build StoreManagement.sln

Write-Host "== EF migration check =="
dotnet ef migrations list --project src/StoreManagement.Infrastructure --startup-project src/StoreManagement.Api

Write-Host "Backend checks completed."
