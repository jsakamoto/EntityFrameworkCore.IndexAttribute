# IndexAttribute for EntityFramework Core [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.EntityFrameworkCore.IndexAttribute.svg)](https://www.nuget.org/packages/Toolbelt.EntityFrameworkCore.IndexAttribute/)

## What's this?

Revival of `[Index]` attribute for EF Core. (with extension for model building.)


### Attention

EF Core team said:

> _"We didn't bring this (= IndexAttribute) over from EF6.x **because it had a lot of issues**"_  
> (https://github.com/aspnet/EntityFrameworkCore/issues/4050)

Therefore, you should consider well before use this package.

## How to use?

1. Annotate your model with `[Index]` attribute that lives in `Toolbelt.ComponentModel.DataAnnotations.Schema` namespace.

```csharp
using Toolbelt.ComponentModel.DataAnnotations.Schema;

public class Person
{
    public int Id { get; set; }

    [Index] // <- Here!
    public string Name { get; set; }
}
```

2. **[Important]** Override `OnModelCreating()` method of your DbContext class, and call `BuildIndexesFromAnnotations()` extension method which lives in `Toolbelt.ComponentModel.DataAnnotations` namespace.

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

That's all!

`BuildIndexesFromAnnotations()` extension method scans the DbContext with .NET Reflection technology, and detects `[Index]` attributes, then build models related to indexing.

After doing that, the database which created by EF Core, contains indexes that are specified by `[Index]` attributes.

## For More Detail...

This library is designed to have the same syntax as EF 6.x `[Index]` annotation.

Please visit document site of EF 6.x and `[Index]` attribute for EF 6.x.

- [MSDN Document - Entity Framework Code First Data Annotations](https://msdn.microsoft.com/en-us/library/jj591583%28v=vs.113%29.aspx?f=255&MSPPError=-2147217396)
- [MSDN Document - IndexAttribute class](https://msdn.microsoft.com/library/system.componentmodel.dataannotations.schema.indexattribute(v=vs.113).aspx)

## Not Supported Feature

`IsClustered` property is not supported at this version.

## License

[MIT License](LICENSE)

