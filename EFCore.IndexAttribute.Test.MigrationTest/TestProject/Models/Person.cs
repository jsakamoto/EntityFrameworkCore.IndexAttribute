using System.ComponentModel.DataAnnotations;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace TestProject.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Index(IsUnique = true), Required, StringLength(10)]
        public string Name { get; set; }

        [Index(IsUnique = true, Includes = new[] { nameof(Weight) })]
        public int Height { get; set; }

        public int Weight { get; set; }
    }
}
