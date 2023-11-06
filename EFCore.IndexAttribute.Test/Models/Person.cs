using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class Person
    {
        public int Id { get; set; }

        [IndexColumn(IsUnique = true)]
        public string Name { get; set; } = "";

        public int Age { get; set; }

        public Address Address { get; set; } = new Address();

        public PhoneNumber PhoneNumber { get; set; } = new PhoneNumber();

        public PhoneNumber FaxNumber { get; set; } = new PhoneNumber();
    }
}
