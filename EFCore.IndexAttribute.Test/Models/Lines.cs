using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace EntityFrameworkCore.IndexAttributeTest.Models;

public class Lines
{
    [IndexColumn("IX_Lines", 1)]
    public string Line1 { get; set; } = "";

    [IndexColumn("IX_Lines", 2)]
    public string Line2 { get; set; } = "";

    public string Line3 { get; set; } = "";
}
