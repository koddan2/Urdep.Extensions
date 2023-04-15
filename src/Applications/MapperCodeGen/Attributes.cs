namespace MapperCodeGen;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class MapFromAttribute : Attribute
{
    public MapFromAttribute(string symbolName)
    {
        SymbolName = symbolName;
    }

    public string SymbolName { get; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MapPropertyAttribute : Attribute
{
    public MapPropertyAttribute(string symbolName)
    {
        SymbolName = symbolName;
    }

    public string SymbolName { get; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class GenerateMappedDtoAttribute : Attribute
{
}
