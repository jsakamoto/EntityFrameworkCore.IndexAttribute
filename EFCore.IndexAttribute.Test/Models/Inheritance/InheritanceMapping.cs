namespace EntityFrameworkCore.IndexAttributeTest.Models.Inheritance;

public enum InheritanceMapping
{
    /// <summary>Table per hierarchy (EF Core default).</summary>
    Tph,

    /// <summary>Table per type.</summary>
    Tpt,

    /// <summary>Table per concrete type.</summary>
    Tpc,
}
