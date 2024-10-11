using System.ComponentModel.DataAnnotations;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace TestProject.Models;

public class Person
{
    public int Id { get; set; }

    [IndexColumn(IsUnique = true), Required, StringLength(10)]
    public string Name { get; set; }

    [IndexColumn(IsUnique = true, Includes = new[] { nameof(Weight) })]
    public int Height { get; set; }

    public int Weight { get; set; }
}
