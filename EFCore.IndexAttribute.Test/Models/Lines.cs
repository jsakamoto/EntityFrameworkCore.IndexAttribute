using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class Lines
    {
        [Index("IX_Lines", 1)]
        public string Line1 { get; set; }

        [Index("IX_Lines", 2)]
        public string Line2 { get; set; }

        public string Line3 { get; set; }
    }
}
