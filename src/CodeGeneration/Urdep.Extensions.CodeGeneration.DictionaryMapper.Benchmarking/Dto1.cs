namespace Urdep.Extensions.CodeGeneration.DictionaryMapper.Benchmarking
{
    [GenerateDictionaryMappingExtensionMethods]
    public record Dto1(int Id, string Name)
    {
        public DateTime? DateTime { get; set; }
        public Version Version { get; set; } = Version.Parse("1.0.0");

        public byte Byte { get; set; } = 0xff;
        public long Long { get; set; } = 0xff;
        public short Short { get; set; } = 0xff;
        public decimal Decimal { get; set; } = 0xff;
    }

    [GenerateDictionaryMappingExtensionMethods]
    public record Dto2(object O1, object O2)
    {
        public object O3 { get; set; } = new object();
        public object O4 { get; set; } = new object();
        public object O5 { get; set; } = new object();
        public object O6 { get; set; } = new object();
        public object O7 { get; set; } = new object();
        public object O8 { get; set; } = new object();
        public object O9 { get; set; } = new object();
    }
}
