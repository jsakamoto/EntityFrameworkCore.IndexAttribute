using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace EntityFrameworkCore.IndexAttributeTest.Models.Inheritance;

/// <summary>
/// The key is named "Code" on purpose, so EF Core's key convention can't find it. The key exists only if [PrimaryKey] works.
/// </summary>
public abstract class ItemBase
{
    [PrimaryKey(IsClustered = false)]
    public string Code { get; set; } = "";

    [IndexColumn]
    public string Name { get; set; } = "";
}

public class Item : ItemBase
{
    public string Kind { get; set; } = "";
}

public class Book : Item
{
    [IndexColumn]
    public string Isbn { get; set; } = "";
}

public class Gadget : Item
{
    public string Maker { get; set; } = "";
}
