using BenchmarkDotNet.Running;

namespace Urdep.Extensions.CodeGeneration.DictionaryMapper.Benchmarking
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmark1>();
        }
    }
}
