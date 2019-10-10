using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class PhoneNumber
    {
        [Index]
        public int CountryCode { get; set; }

        public string CityNumber { get; set; }

        public string Number { get; set; }
    }
}
