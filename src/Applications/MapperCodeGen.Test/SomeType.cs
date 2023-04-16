namespace MapperCodeGen.Test;

////[GenerateMappedDto]
////internal class SomeType
////{
////    public int Age { get; set; }
////    public string Name { get; set; } = "";
////}

internal class SomeExternalType
{
    public Guid Id { get; set; }
    public int Age { get; set; }
    public string Name { get; set; } = "";
}

[MapFrom(nameof(SomeExternalType))]
internal class SomeCustomType
{
    [MapProperty(nameof(SomeExternalType.Id))]
    public Guid Id { get; set; }
    public int Age { get; set; }
    public string Name { get; set; } = "";
}
