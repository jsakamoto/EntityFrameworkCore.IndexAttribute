using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }

        public string ZipPostCode { get; set; }
        public string TownCity { get; set; }

        public string Country { get; set; }
    }
}
