using System;

namespace Urdep.Extensions.CodeGeneration.DictionaryMapper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class GenerateDictionaryMappingExtensionMethodsAttribute : Attribute { }
}
