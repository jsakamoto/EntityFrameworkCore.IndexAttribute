# IndexColumnAttribute for EntityFramework Core  

## Appendix D - Upgrade an existing project

To upgrade an existing project that uses ver.3 or before to use ver.5 or later of this package:
1. Please confirm that the version of this package you use is ver.5 or later.

```
PM> Update-Package Toolbelt.EntityFrameworkCore.IndexAttribute
```

2. Remove `using Toolbelt.ComponentModel.DataAnnotations.Schema;`, and insert `using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;` instead.

```csharp
...
// 👇 Remove this line...
// using Toolbelt.ComponentModel.DataAnnotations.Schema;

// 👇 Insert this line, instead.
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;
...
```

3. Replace `[Index]` attribute to `[IndexColumn]` attribute.

```csharp
...
public class Foo {
  ...
  // 👇 Replace [Index] to [IndexColumn]
  [IndexColumn] 
  public int Bar { get; set; }
  ...
```
