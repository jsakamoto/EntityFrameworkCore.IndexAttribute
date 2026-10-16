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
}
