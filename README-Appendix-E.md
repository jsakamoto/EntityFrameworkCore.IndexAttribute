# IndexColumnAttribute for EntityFramework Core

## Appendix E -  If you run into a compile error CS0104...

If you run into a compile error CS0104 "'Index' is an ambiguous reference between 'Toolbelt.ComponentModel.DataAnnotations.Schema.IndexAttribute' and 'Microsoft.EntityFrameworkCore.IndexAttribute'" in your project that has to use the old version of this package (ver.3.x or before),

```csharp
using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

public class Foo {
  ...
  [Index] // 👈 CS0104 "'Index' is an ambiguous reference...
  public int Bar { get; set; }
}
```

you can resolve this compile error by the following workaround steps.

1. Remove `using namespace` directive.

```csharp
// 👇 Remove this line...
using Toolbelt.ComponentModel.DataAnnotations.Schema;
```

2. Insert `using alias = full qualified name` directive to add the alias of `Toolbelt.ComponentModel.DataAnnotations.Schema.IndexAttribute` class.

```csharp
// 👇 Insert this line instead to add the alias.
using IndexColumnAttribute =
  Toolbelt.ComponentModel.DataAnnotations.Schema.IndexAttribute;
```

3. Replace `[Index]` to `[IndexColumn]`.

```csharp
  ...
  // 👇 Replace [Index] to [IndexColumn]
  [IndexColumn] 
  public int Bar { get; set; }
  ...
```

Finally, the example code will be as below, and you can compile it as expected.

```csharp
using Microsoft.EntityFrameworkCore;
using IndexColumnAttribute =
    Toolbelt.ComponentModel.DataAnnotations.Schema.IndexAttribute;

public class Foo {
  ...
  [IndexColumn]
  public int Bar { get; set; }
}
```
