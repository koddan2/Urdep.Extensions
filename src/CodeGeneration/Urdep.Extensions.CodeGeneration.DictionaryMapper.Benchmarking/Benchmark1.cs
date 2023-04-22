using BenchmarkDotNet.Attributes;
using System.Text.Json;

using Urdep.Extensions.CodeGeneration.DictionaryMapper.Benchmarking.DictionaryMapping;

namespace Urdep.Extensions.CodeGeneration.DictionaryMapper.Benchmarking
{
    public class Benchmark1
    {
        private static Dto1 _DataObject1 => new(42, "Gandalf") { DateTime = DateTime.Now, };
        private static Dto2 _DataObject2 => new(new object(), new object());

        private static readonly Dictionary<string, object?> _StaticDict1 = new();
        private static readonly Dictionary<string, object?> _StaticDict2 = new();

        static Benchmark1()
        {
            _DataObject1.IntoDictionary(_StaticDict1);
            _DataObject2.IntoDictionary(_StaticDict2);
        }

#pragma warning disable CA1822 // Mark members as static
        [Benchmark]
        public string FromObject1() => JsonSerializer.Serialize(_DataObject1);

        [Benchmark]
        public string FromToDictionary1() => JsonSerializer.Serialize(_DataObject1.ToDictionary());

        [Benchmark]
        public string FromDictionary1()
        {
            return JsonSerializer.Serialize(_StaticDict1);
        }

        [Benchmark]
        public string FromObject2() => JsonSerializer.Serialize(_DataObject2);

        [Benchmark]
        public string FromToDictionary2() => JsonSerializer.Serialize(_DataObject2.ToDictionary());

        [Benchmark]
        public string FromDictionary2()
        {
            return JsonSerializer.Serialize(_StaticDict2);
        }
#pragma warning restore CA1822 // Mark members as static
    }
}
