using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Index(IsUnique = true)]
        public string Name { get; set; }

        public int Age { get; set; }

        public Address Address { get; set; }

        public PhoneNumber PhoneNumber { get; set; }

        public PhoneNumber FaxNumber { get; set; }
    }
}
