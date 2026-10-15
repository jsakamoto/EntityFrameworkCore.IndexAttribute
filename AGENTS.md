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
- Tests use xUnit v3 on Microsoft.Testing.Platform (`global.json` `test.runner`; needs .NET 11 SDK to build `net11.0`). Run `dotnet test` inside the test project folder. The .NET 11 RC SDK resolves `dotnet test --project <folder>` against the wrong path and fails. Assertions (`.Is(...)`) come from `ChainingAssertion-xUnit.Bin` 3.0.0 (`using Xunit;`).
- A Release build produces `.nupkg` files into `dist/` (`GeneratePackageOnBuild`, git-ignored).

## Architecture

Three packaged projects, all `RootNamespace` `Toolbelt.ComponentModel.DataAnnotations.Schema`:

- `EFCore.IndexAttribute.Attribute` (netstandard2.0): dependency-free attribute definitions. `V5/IndexColumnAttribute` and `V5/PrimaryKeyAttribute` are the current ones, in the `...Schema.V5` namespace to avoid clashing with EF Core 5's own `[Index]`. The root `IndexAttribute` and `PrimaryKeyAttribute` are `[Obsolete]` legacy types, kept for compatibility. `INameAndOrder` (internal) is the shared shape the builder uses. `InternalsVisibleTo` exposes it to the main package.
- `EFCore.IndexAttribute` (netstandard2.0/2.1, net10.0, net11.0): `AttributedIndexBuilderExtension.BuildIndexesFromAnnotations()`. It uses `Toolbelt.EntityFrameworkCore.AnnotationBasedModelBuilder` (an external package) to scan entities, including owned types, then groups attributes by `Name` and orders them by `Order` to form (composite) indexes. It also calls `HasKey` for `[PrimaryKey]`. The root-namespace `IndexAttribute.cs` is only a `TypeForwardedTo` to the Attribute assembly.
  - EF Core 5 and later have their own `IndexAttribute` and `PrimaryKeyAttribute` in `Microsoft.EntityFrameworkCore`. The library compiles against EF Core 3.x, so they do not clash today. `AttributedIndexBuilderExtension.cs` still has `using` aliases for the Toolbelt types, in case it is ever compiled against a newer EF Core.
- `EFCore.IndexAttribute.SqlServer` (netstandard2.0/2.1, net10.0, net11.0): `BuildIndexesFromAnnotationsForSqlServer()` calls the internal overload of `BuildIndexesFromAnnotations` with post-process callbacks that apply `IsClustered` and `IncludeProperties`. It also forces `SuppressNotSupportedException` on. The base method throws `NotSupportedException` for `IsClustered` or `Includes` unless suppressed.

Design constraint: SQL Server-only features live only in the `.SqlServer` project, so the core package does not depend on `Microsoft.EntityFrameworkCore.SqlServer`. Keep it that way.

## Target frameworks and EF Core versions

Follow these three rules.

1. **The libraries list each .NET version from `net10.0` on.** `EFCore.IndexAttribute` and `EFCore.IndexAttribute.SqlServer` target `netstandard2.0` and `netstandard2.1`, plus `net10.0` and `net11.0` one by one. When a new .NET major version ships, add its TFM. These TFMs are there so that nuget.org shows them as supported. (`EFCore.IndexAttribute.Attribute` has no EF dependency and stays `netstandard2.0` only.)
2. **The libraries keep the oldest EF Core reference on every TFM.** `EFCore.IndexAttribute` has no direct EF Core reference. EF Core comes through the AnnotationBasedModelBuilder package, which depends on EF Core Relational 3.0.0 (3.1.0 for `netstandard2.0`). `EFCore.IndexAttribute.SqlServer` references EF Core SqlServer 3.0.0 (3.1.0 for `netstandard2.0`). This keeps one package usable with EF Core 3.0 through the latest. Never raise these references, not even for `net10.0` or later. A newer reference would force users to upgrade EF Core, for example a .NET 10 app that still uses EF Core 8.
3. **The test project and `TestProject` use the latest EF Core of the same major version as each TFM.** Both target `net8.0` to `net11.0`, and each TFM has a conditional `PackageReference` group (for example, `net10.0` uses the latest EF Core 10.0.x). The other Microsoft packages in the group (`System.Runtime.Caching`, `Microsoft.Extensions.Logging.Debug`) follow the same rule. While a .NET version is still in preview or RC, use its latest prerelease (now `11.0.0-rc.2.26504.105`). When you bump versions, update every group in both projects.

## Release conventions

- All three packages share a `Version` (currently 5.0.1.3), set in each csproj. `TestProject`'s package references must match.
- Each project has a `RELEASE-NOTES.txt`. A `BeforeTargets="GenerateNuspec"` MSBuild target regex-extracts the topmost `v.x.y.z` section into `PackageReleaseNotes`, so new entries go on top and start with `v.` followed by a version.
