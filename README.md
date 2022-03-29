# IndexColumnAttribute for EntityFramework Core [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.EntityFrameworkCore.IndexAttribute.svg)](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.IndexAttribute/) [![unit tests](https://github.com/jsakamoto/EntityFrameworkCore.IndexAttribute/actions/workflows/unit-tests.yml/badge.svg?branch=master&event=push)](https://github.com/jsakamoto/EntityFrameworkCore.IndexAttribute/actions/workflows/unit-tests.yml)

## What's this?

The `[IndexColumn]` attribute that is the revival of `[Index]` attribute for EF Core. (with extension for model building.)

### Attention

EF Core also includes the `[Index]` attribute officially, after ver.5.0.

However, I'm going to continue improving and maintaining these libraries, because these libraries still have advantages as below.

- You can still create indexes by data annotations even if you have to use **a lower version of EF Core**.
- You can create indexes with **"included columns"** for SQL Server.
- You can create a **clustered index** (This means you can also create a non-clustered primary key index).

## How to use?

1. Add [`Toolbelt.EntityFrameworkCore.IndexAttribute`](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.IndexAttribute/) package to your project.

```shell
> dotnet add package Toolbelt.EntityFrameworkCore.IndexAttribute
```

### Supported Versions

The version of EF Core | Version of this package
-----------------------|-------------------------
v.7.0                  | **v.5.0.1 (Recommended)**, v.5.0, v.3.2, v.3.1
v.6.0                  | **v.5.0.1 (Recommended)**, v.5.0, v.3.2, v.3.1
v.5.0                  | **v.5.0.1 (Recommended)**, v.5.0, v.3.2, v.3.1
v.3.1                  | **v.5.0.1 (Recommended)**, v.5.0, v.3.2, v.3.1
v.3.0                  | **v.5.0.1 (Recommended)**, v.5.0, v.3.2, v.3.1, v.3.0
v.2.0, 2.1, 2.2        | v.2.0.x

If you want to use `IsClustered=true` and/or `Includes` index features on a SQL Server, you have to add [`Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer`](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer/) package to your project, instead.

```shell
> dotnet add package Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer
```

2. Annotate your model with `[IndexColumn]` attribute that lives in `Toolbelt.ComponentModel.DataAnnotations.Schema.V5` namespace.

_**Notice**_ - The attribute name is **`[IndexColumn]`**, is not `[Index]` (the attribute name `[Index]` is used by EFocre v.5.0).

```csharp
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

public class Person
{
    public int Id { get; set; }

    [IndexColumn] // <- Here!
    public string Name { get; set; }
}
```

3. **[Important]** Override `OnModelCreating()` method of your DbContext class, and call `BuildIndexesFromAnnotations()` extension method which lives in `Toolbelt.ComponentModel.DataAnnotations` namespace.

```csharp
using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations;

public class MyDbContext : DbContext
{
  ...
  // Override "OnModelCreating", ...
  protected override void OnModelCreatin(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // .. and invoke"BuildIndexesFromAnnotations"!
    modelBuilder.BuildIndexesFromAnnotations();
  }
}
```

If you use SQL Server and `IsClustered=true` and/or `Includes = new[]{"Foo", "Bar"}` features, you need to call `BuildIndexesFromAnnotationsForSqlServer()` extension method instead of `BuildIndexesFromAnnotations()` extension method.

```csharp
  ...
  // Override "OnModelCreating", ...
  protected override void OnModelCreatingModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Invoke uildIndexesFromAnnotationsForSqlServer"
    // instead of "BuildIndexesFromAnnotations".
    modelBuilder.BuildIndexesFromAnnotationsForSqlServer;
  }
```

That's all!

`BuildIndexesFromAnnotations()` (or, `BuildIndexesFromAnnotationsForSqlServer()`) extension method scans the DbContext with .NET Reflection technology, and detects `[IndexColumn]` attributes, then build models related to indexing.

After doing that, the database which created by EF Core, contains indexes that are specified by `[IndexColumn]` attributes.

## Appendix A - Suppress "NotSupportedException"

You will run into "NotSupportedException" when you call `BuildIndexesFromAnnotations()` with the model which is annotated with the `[IndexColumn]` attribute that's "IsClustered" property is true, or "Includes" property is not empty.

If you have to call `BuildIndexesFromAnnotations()` in this case (for example, share the model for different Database products), you can suppress this behavior with configuration, like this.

```csharp
  ...
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Suppress "NotSupportedException" for "IsClustered" and "Includes" feature.
    modelBuilder.BuildIndexesFromAnnotations(options => {
      options.SuppressNotSupportedException.IsClustered = true;
      options.SuppressNotSupportedException.Includes = true;
    });
  }
}
```

## Appendix B -  Notice for using "IsClustered=true"

If you annotate the model with "IsClustered=true" index simply like this,

```csharp
public class Employee {
  public int Id { get; set; }

  [IndexColumn(IsClustered = true)]
  public string EmployeeCode { get; set; }
}
```

You will run into 'System.Data.SqlClient.SqlException' like this.

```
System.Data.SqlClient.SqlException :
Cannot create more than one clustered index on table '(table name)'.
Drop the existing clustered index '(index name)' before creating another.
```

In this case, you need to annotate a primary key property with `[PrimaryKey(IsClustered = false)]` attribute explicitly  for suppress auto generated primary key to be clustered index.

```csharp
public class Employee {
  [PrimaryKey(IsClustered = false)] // <- Add this line!
  public int Id { get; set; }

  [IndexColumn(IsClustered = true)]
  public string EmployeeCode { get; set; }
}
```

## Appendix C -  If you want to use only "IndexAttribute" without any dependencies...

If you want to use only "IndexColumnAttribute" class without any dependencies, you can use [Toolbelt.EntityFrameworkCore.IndexAttribute.Attribute](https://j.mp/3kfJgTm) NuGet package.

## Appendix D - Upgrade an existing project

For more detail on this topic, please visit [this link.](https://j.mp/2HlmNFJ)

## Appendix E -  If you run into a compile error CS0104...

For more detail on this topic, please visit [this link.](https://j.mp/3476B3X)

## For More Detail...

This library is designed to have the same syntax as EF 6.x `[Index]` annotation.

Please visit document site of EF 6.x and `[Index]` attribute for EF 6.x.

- [MSDN Document - Entity Framework Code First Data Annotations](https://j.mp/37hHBZI)
- [MSDN Document - IndexAttribute class](https://j.mp/2HeIAzp)

## Limitations

`[IndexColumn]` attribute with `IsClustered=true` can apply only not owned entity types.

## Release Notes

- [Toolbelt.EntityFrameworkCore.IndexAttribute.Attibute](https://j.mp/3lSWUfw)
- [Toolbelt.EntityFrameworkCore.IndexAttribute](https://j.mp/359Hg90)
- [Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer](https://j.mp/3dBWDuu)

## License

[MIT License](https://j.mp/3476mWB)

