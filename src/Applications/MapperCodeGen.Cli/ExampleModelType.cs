using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MapperCodeGen.Cli;

internal interface IHasAge
{
    int Age { get; }
}

[DataContract]
internal class ExampleModelType : IHasAge
{
    [Range(0, 100)]
    public int Age { get; set; }

    [Required]
    [MaxLength(0xff)]
    public string? Name { get; set; }

    public ICollection<string> Tags { get; set; } = new HashSet<string>();

    public Address? Address { get; set; }
}

internal class Address
{
    public Address(string city)
    {
        City = city;
    }

    public string City { get; set; }
}
