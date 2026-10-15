# IndexColumnAttribute for Entity Framework Core [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.EntityFrameworkCore.IndexAttribute.svg)](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.IndexAttribute/) [![unit tests](https://github.com/jsakamoto/EntityFrameworkCore.IndexAttribute/actions/workflows/unit-tests.yml/badge.svg?branch=master&event=push)](https://github.com/jsakamoto/EntityFrameworkCore.IndexAttribute/actions/workflows/unit-tests.yml)

## What's this?

This library brings the `[Index]` attribute of EF 6 back to EF Core, as the `[IndexColumn]` attribute. It also includes extension methods that build the indexes in your model from these attributes.

The automated tests check that it works on **SQL Server**, **PostgreSQL**, and **SQLite**.

### Attention

Since ver.5.0, EF Core has its own official `[Index]` attribute.

However, I will keep maintaining and improving these libraries, because they still have some advantages.

- You can create indexes with data annotations even on **an older version of EF Core**.
- You can create indexes with **included columns** on SQL Server.
- You can create a **clustered index**. This means you can also create a non-clustered primary key.

## How to use?

1. Add the [`Toolbelt.EntityFrameworkCore.IndexAttribute`](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.IndexAttribute/) package to your project.

```shell
> dotnet add package Toolbelt.EntityFrameworkCore.IndexAttribute
```

### Supported Versions

EF Core version              | Package version
-----------------------------|-------------------------
v.3.1, 5, 6, 7, ...11        | **v.5.0.1 or later (Recommended)**, v.5.0, v.3.2, v.3.1
v.3.0                        | **v.5.0.1 or later (Recommended)**, v.5.0, v.3.2, v.3.1, v.3.0
v.2.0, 2.1, 2.2              | v.2.0.x

If you want to use the `IsClustered=true` or `Includes` features on SQL Server, add the [`Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer`](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer/) package to your project instead.

```shell
> dotnet add package Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer
```

2. Annotate your model with the `[IndexColumn]` attribute. It lives in the `Toolbelt.ComponentModel.DataAnnotations.Schema.V5` namespace.

_**Notice**_ - The attribute name is **`[IndexColumn]`**, not `[Index]`. EF Core 5.0 and later already use the name `[Index]`.

```csharp
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

public class Person
{
    public int Id { get; set; }

    [IndexColumn] // <- Here!
    public string Name { get; set; }
}
```

3. **[Important]** Override the `OnModelCreating()` method of your DbContext class, and call the `BuildIndexesFromAnnotations()` extension method. It lives in the `Toolbelt.ComponentModel.DataAnnotations` namespace.

```csharp
using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations;

public class MyDbContext : DbContext
{
  ...
  // Override "OnModelCreating", ...
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // .. and invoke "BuildIndexesFromAnnotations"!
    modelBuilder.BuildIndexesFromAnnotations();
  }
}
```

If you use the `IsClustered=true` or `Includes = new[]{"Foo", "Bar"}` features on SQL Server, call the `BuildIndexesFromAnnotationsForSqlServer()` extension method instead of `BuildIndexesFromAnnotations()`.

```csharp
  ...
  // Override "OnModelCreating", ...
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Invoke "BuildIndexesFromAnnotationsForSqlServer"
    // instead of "BuildIndexesFromAnnotations".
    modelBuilder.BuildIndexesFromAnnotationsForSqlServer();
  }
```

That's all!

The `BuildIndexesFromAnnotations()` (or `BuildIndexesFromAnnotationsForSqlServer()`) extension method scans the DbContext with .NET reflection, finds the `[IndexColumn]` attributes, and builds the indexes in the model.

As a result, the database that EF Core creates has the indexes that you specified with the `[IndexColumn]` attributes.

## Appendix A - Suppress "NotSupportedException"

You will get a "NotSupportedException" when you call `BuildIndexesFromAnnotations()` with a model that has an `[IndexColumn]` attribute whose "IsClustered" property is true, or whose "Includes" property is not empty.

If you still need to call `BuildIndexesFromAnnotations()` in this case (for example, to share the model between different database products), you can suppress the exception with options, like this.

```csharp
  ...
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Suppress "NotSupportedException" for "IsClustered" and "Includes" features.
    modelBuilder.BuildIndexesFromAnnotations(options => {
      options.SuppressNotSupportedException.IsClustered = true;
      options.SuppressNotSupportedException.Includes = true;
    });
  }
}
```

## Appendix B - Notes on "IsClustered=true"

Suppose you simply annotate the model with "IsClustered=true", like this.

```csharp
public class Employee {
  public int Id { get; set; }

  [IndexColumn(IsClustered = true)]
  public string EmployeeCode { get; set; }
}
```

Then you will get a 'System.Data.SqlClient.SqlException' like this.

```
System.Data.SqlClient.SqlException :
Cannot create more than one clustered index on table '(table name)'.
Drop the existing clustered index '(index name)' before creating another.
```

In this case, annotate the primary key property with the `[PrimaryKey(IsClustered = false)]` attribute explicitly. This stops the primary key from becoming a clustered index automatically.

```csharp
public class Employee {
  [PrimaryKey(IsClustered = false)] // <- Add this line!
  public int Id { get; set; }

  [IndexColumn(IsClustered = true)]
  public string EmployeeCode { get; set; }
}
```

## Appendix C - If you want to use only "IndexAttribute" without any dependencies...

If you want to use only the "IndexColumnAttribute" class without any dependencies, use the [Toolbelt.EntityFrameworkCore.IndexAttribute.Attribute](https://j.mp/3kfJgTm) NuGet package.

## Appendix D - Upgrade an existing project

For more details on this topic, see [this link](https://j.mp/2HlmNFJ).

## Appendix E - If you run into a compile error CS0104...

For more details on this topic, see [this link](https://j.mp/3476B3X).

## For More Details...

This library uses the same syntax as the `[Index]` attribute of EF 6.x.

See the EF 6.x documents below for more details.

- [MSDN Document - Entity Framework Code First Data Annotations](https://j.mp/37hHBZI)
- [MSDN Document - IndexAttribute class](https://j.mp/2HeIAzp)

## Limitations

The `[IndexColumn]` attribute with `IsClustered=true` works only on entity types that are not owned types.

Don't give a name to `[PrimaryKey]` on a base class property, such as `[PrimaryKey("PK_Items")]`, when you use the table-per-type (TPT) or table-per-concrete-type (TPC) mapping. EF Core gives that one name to the primary key of every table in the hierarchy, so SQL Server and PostgreSQL fail to create the tables because of the duplicate name. Use `[PrimaryKey]` without a name instead. Then EF Core names each primary key after its own table.

## Release Notes

- [Toolbelt.EntityFrameworkCore.IndexAttribute.Attribute](https://j.mp/3lSWUfw)
- [Toolbelt.EntityFrameworkCore.IndexAttribute](https://j.mp/359Hg90)
- [Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer](https://j.mp/3dBWDuu)

## License

[MIT License](https://j.mp/3476mWB)

