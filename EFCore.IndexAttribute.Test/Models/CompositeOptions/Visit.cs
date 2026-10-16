using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace EntityFrameworkCore.IndexAttributeTest.Models.CompositeOptions;

/// <summary>
/// Issue #15. Each option is set only on a column that is not the first one of its index, and it must still apply to the whole index.
/// </summary>
public class Visit
{
    [PrimaryKey]
    public int Id { get; set; }

    [IndexColumn("IX_Unique", 0)]
    public int SiteId { get; set; }

    [IndexColumn("IX_Unique", 1, IsUnique = true)]
    public int VisitorId { get; set; }

    [IndexColumn("IX_Clustered", 0)]
    public int PageId { get; set; }

    [IndexColumn("IX_Clustered", 1, IsClustered = true)]
    public int Sequence { get; set; }

    [IndexColumn("IX_Includes", 0, Includes = new[] { nameof(Referrer) })]
    public int CampaignId { get; set; }

    [IndexColumn("IX_Includes", 1, Includes = new[] { nameof(UserAgent) })]
    public int SourceId { get; set; }

    public string Referrer { get; set; } = "";

    public string UserAgent { get; set; } = "";
}

/// <summary>
/// Not an entity type. The options are set only on the base class part of "IX_Tenant_Slug", and they must apply to the whole index.
/// </summary>
public abstract class ArticleBase
{
    [PrimaryKey]
    public int Id { get; set; }

    [IndexColumn("IX_Tenant_Slug", 0, IsUnique = true, IsClustered = true)]
    public int TenantId { get; set; }
}

public class Article : ArticleBase
{
    [IndexColumn("IX_Tenant_Slug", 1)]
    public string Slug { get; set; } = "";
}

public class OrderLine
{
    [PrimaryKey("PK_OrderLines", 0)]
    public int OrderId { get; set; }

    [PrimaryKey("PK_OrderLines", 1, IsClustered = true)]
    public int LineNumber { get; set; }
}
