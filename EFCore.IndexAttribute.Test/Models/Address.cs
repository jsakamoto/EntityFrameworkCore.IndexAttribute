using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace EntityFrameworkCore.IndexAttributeTest.Models;

public class Address
{
    public Lines Lines { get; set; } = new Lines();

    public string ZipPostCode { get; set; } = "";

    public string TownCity { get; set; } = "";

    [IndexColumn("IX_Country", Includes = new[] { nameof(ZipPostCode), nameof(TownCity) })]
    public string Country { get; set; } = "";
}
