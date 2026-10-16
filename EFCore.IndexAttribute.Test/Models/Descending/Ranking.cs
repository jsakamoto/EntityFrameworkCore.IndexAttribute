using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace EntityFrameworkCore.IndexAttributeTest.Models.Descending;

public class Ranking
{
    public int Id { get; set; }

    [IndexColumn(IsDescending = true)]
    public DateTime CreatedAt { get; set; }

    [IndexColumn("IX_Category_Score", 0)]
    public string Category { get; set; } = "";

    [IndexColumn("IX_Category_Score", 1, IsDescending = true)]
    public int Score { get; set; }

    public RankingDetail Detail { get; set; } = new();
}

[Owned]
public class RankingDetail
{
    [IndexColumn(IsDescending = true)]
    public DateTime UpdatedAt { get; set; }

    [IndexColumn("IX_Group_Level", 0)]
    public int Group { get; set; }

    [IndexColumn("IX_Group_Level", 1, IsDescending = true)]
    public int Level { get; set; }
}
