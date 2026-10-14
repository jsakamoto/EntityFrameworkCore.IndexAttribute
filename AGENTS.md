# AGENTS.md

This file provides guidance to AI coding agents when working with code in this repository.

## Overview

NuGet libraries that revive an `[Index]`-style data annotation for EF Core, as `[IndexColumn]` and `[PrimaryKey]`, plus `ModelBuilder` extensions that turn the annotations into EF Core model configuration. `README.md` is the user-facing doc and is also packed into every NuGet package.

## Commands

```shell
dotnet build EFCore.IndexAttribute.slnx
cd EFCore.IndexAttribute.Test
dotnet test                                                              # all target frameworks (net8.0/9.0/10.0/11.0)
dotnet test -f net8.0 --filter-class "*IndexAttributeTest_on_SQLite"
```

- SQL Server tests (`IndexAttributeTest`) read `MSSQL_SERVER`, `MSSQL_USER`, `MSSQL_PWD` (CI uses `localhost,1433` / `sa` / `Passw0rd` against a SQL Server docker container). The SQLite tests need nothing external.
- `MigrationTest` copies `EFCore.IndexAttribute.Test/TestProject` to a temp dir and runs `dotnet build` and `dotnet ef migrations add`. `TestProject` is excluded from the test project's compilation and references the packed NuGet packages (`Version` in its csproj), restored from the local `dist/` feed (`nuget.config`). Build in Release first so `dist/` holds the packages, and keep the versions in sync. `TestProject` also targets net8.0 to net11.0. EF Core 11 needs `Microsoft.Data.SqlClient` 7.0.2 or later, so keep its SqlClient reference at that level or higher.
- Tests use xUnit v3 on Microsoft.Testing.Platform (`global.json` `test.runner`; needs .NET 11 SDK to build `net11.0`). Run `dotnet test` inside the test project folder. The .NET 11 RC SDK resolves `dotnet test --project <folder>` against the wrong path and fails.
- .NET 11 is still in RC, so every `net11.0` package reference (libraries, tests and `TestProject`) points to `11.0.0-rc.2.26504.105`. When a new RC or the GA ships, update all of them together. Assertions (`.Is(...)`) come from `ChainingAssertion-xUnit.Bin` 3.0.0 (`using Xunit;`).
- A Release build produces `.nupkg` files into `dist/` (`GeneratePackageOnBuild`, git-ignored).

## Architecture

Three packaged projects, all `RootNamespace` `Toolbelt.ComponentModel.DataAnnotations.Schema`:

- `EFCore.IndexAttribute.Attribute` (netstandard2.0): dependency-free attribute definitions. `V5/IndexColumnAttribute` and `V5/PrimaryKeyAttribute` are the current ones, in the `...Schema.V5` namespace to avoid clashing with EF Core 5's own `[Index]`. The root `IndexAttribute` and `PrimaryKeyAttribute` are `[Obsolete]` legacy types, kept for compatibility. `INameAndOrder` (internal) is the shared shape the builder uses. `InternalsVisibleTo` exposes it to the main package.
- `EFCore.IndexAttribute` (netstandard2.0/2.1, net10.0, net11.0): `AttributedIndexBuilderExtension.BuildIndexesFromAnnotations()`. It uses `Toolbelt.EntityFrameworkCore.AnnotationBasedModelBuilder` (an external package) to scan entities, including owned types, then groups attributes by `Name` and orders them by `Order` to form (composite) indexes. It also calls `HasKey` for `[PrimaryKey]`. The root-namespace `IndexAttribute.cs` is only a `TypeForwardedTo` to the Attribute assembly.
  - The `net10.0` and `net11.0` targets reference EF Core Relational 10 and 11 directly. Without them, the AnnotationBasedModelBuilder package (netstandard assets only) would pull in EF Core 3.x for those targets.
  - EF Core 10 and 11 have their own `IndexAttribute` and `PrimaryKeyAttribute` in `Microsoft.EntityFrameworkCore`. `AttributedIndexBuilderExtension.cs` uses `using` aliases to pick the Toolbelt types, so keep those aliases.
- `EFCore.IndexAttribute.SqlServer` (netstandard2.0/2.1, net10.0, net11.0; the latter two reference EF Core SqlServer 10 and 11): `BuildIndexesFromAnnotationsForSqlServer()` calls the internal overload of `BuildIndexesFromAnnotations` with post-process callbacks that apply `IsClustered` and `IncludeProperties`. It also forces `SuppressNotSupportedException` on. The base method throws `NotSupportedException` for `IsClustered` or `Includes` unless suppressed.

Design constraint: SQL Server-only features live only in the `.SqlServer` project, so the core package does not depend on `Microsoft.EntityFrameworkCore.SqlServer`. Keep it that way.

## Release conventions

- All three packages share a `Version` (currently 5.0.1.3), set in each csproj. `TestProject`'s package references must match.
- Each project has a `RELEASE-NOTES.txt`. A `BeforeTargets="GenerateNuspec"` MSBuild target regex-extracts the topmost `v.x.y.z` section into `PackageReleaseNotes`, so new entries go on top and start with `v.` followed by a version.
- While the `net11.0` targets depend on EF Core 11 RC, packing warns NU5104 (a stable package with a prerelease dependency). Move to the EF Core 11 GA version before you release.
