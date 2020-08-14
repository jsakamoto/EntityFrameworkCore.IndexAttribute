using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class Person
    {
        public int Id { get; set; }

        [IndexColumn(IsUnique = true)]
        public string Name { get; set; }

        public int Age { get; set; }

        public Address Address { get; set; }

        public PhoneNumber PhoneNumber { get; set; }

        public PhoneNumber FaxNumber { get; set; }
    }
}
