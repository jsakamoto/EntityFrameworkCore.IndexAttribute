using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class Address
    {
        public Lines Lines { get; set; }

        public string ZipPostCode { get; set; }

        public string TownCity { get; set; }

        [Index("IX_Country", Includes = new[] { nameof(ZipPostCode), nameof(TownCity) })]
        public string Country { get; set; }
    }
}
