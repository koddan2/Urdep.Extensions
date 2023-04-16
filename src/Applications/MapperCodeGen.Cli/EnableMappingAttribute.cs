namespace MapperCodeGen.Cli;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class EnableMappingAttribute : Attribute { }
