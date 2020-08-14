using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class PhoneNumber
    {
        [IndexColumn]
        public int CountryCode { get; set; }

        public string CityNumber { get; set; }

        public string Number { get; set; }
    }
}
