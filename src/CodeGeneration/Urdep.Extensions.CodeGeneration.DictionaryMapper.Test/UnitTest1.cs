
using Urdep.Extensions.CodeGeneration.DictionaryMapper.Test.DictionaryMapping;
namespace Urdep.Extensions.CodeGeneration.DictionaryMapper.Test
{
    public record SomeComplexThing
    {
        public Version Version { get; set; } = new Version();
    }

    [GenerateDictionaryMappingExtensionMethods]
    public record NotSoSimpleDto(int Id, string Name)
    {
        public SomeComplexThing Thing { get; set; } = new SomeComplexThing()
        {
            Version = Version.Parse("1.0.0.0"),
        };
        public Guid? MaybeGuid { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [GenerateDictionaryMappingExtensionMethods]
    public record SimpleDto(string Name)
    {
        public int? Id { get; set; }
    }

    [GenerateDictionaryMappingExtensionMethods]
    public readonly record struct ReadOnlyRecordStructType1(int Id, string Name);

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var dto = new NotSoSimpleDto(42, "Gandalf");
            var dict = dto.ToDictionary();
            Assert.Multiple(() =>
            {
                Assert.That(dict["Id"], Is.EqualTo(dto.Id));
                Assert.That(dict["Name"], Is.EqualTo(dto.Name));
                Assert.That(dict["Thing"], Is.EqualTo(dto.Thing));
                Assert.That(dict["MaybeGuid"], Is.EqualTo(dto.MaybeGuid));
                Assert.That(dict["CreatedAt"], Is.EqualTo(dto.CreatedAt));
            });

            var dto2 = dict.FromDictionaryToNotSoSimpleDto();
            Assert.That(dto2, Is.EqualTo(dto));
        }

        [Test]
        public void Test6()
        {
            var dto = new NotSoSimpleDto(42, "Gandalf");
            Dictionary<string, object?> dict = new()
            {
                // should be ignored
                ["BOGUS"] = "GARBAGE",
            };
            dto.IntoDictionary(dict);
            Assert.Multiple(() =>
            {
                Assert.That(dict["Id"], Is.EqualTo(dto.Id));
                Assert.That(dict["Name"], Is.EqualTo(dto.Name));
                Assert.That(dict["Thing"], Is.EqualTo(dto.Thing));
                Assert.That(dict["MaybeGuid"], Is.EqualTo(dto.MaybeGuid));
                Assert.That(dict["CreatedAt"], Is.EqualTo(dto.CreatedAt));
            });

            var dto2 = dict.FromDictionaryToNotSoSimpleDto();
            Assert.That(dto2, Is.EqualTo(dto));
        }

        [Test]
        public void Test2()
        {
            Dictionary<string, object?> dict = new()
            {
                ["Name"] = "Test",
            };
            var dto = dict.FromDictionaryToSimpleDto();
            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.Null);
                Assert.That(dto.Name, Is.EqualTo(dict["Name"]));
            });
        }

        [Test]
        public void Test3()
        {
            Dictionary<string, object?> dict = new()
            {
                ["Name"] = "Test",
                ["Id"] = null,
            };
            var dto = dict.FromDictionaryToSimpleDto();
            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.Null);
                Assert.That(dto.Name, Is.EqualTo(dict["Name"]));
            });
        }

        [Test]
        public void Test4()
        {
            Dictionary<string, object?> dict = new()
            {
                ["Name"] = null,
            };
            Assert.Throws<ArgumentException>(() =>
            {
                var dto = dict.FromDictionaryToSimpleDto();
            });
        }

        [Test]
        public void Test5()
        {
            Dictionary<string, object?> dict = new()
            {
            };
            Assert.Throws<KeyNotFoundException>(() =>
            {
                var dto = dict.FromDictionaryToSimpleDto();
            });
        }

        [Test]
        public void Test7()
        {
            var dto = new ReadOnlyRecordStructType1(42, "Gandalf");
            var dict = dto.ToDictionary();
            Assert.Multiple(() =>
            {
                Assert.That(dict["Id"], Is.EqualTo(dto.Id));
                Assert.That(dict["Name"], Is.EqualTo(dto.Name));
            });
            var dto2 = dict.FromDictionaryToReadOnlyRecordStructType1();
            Assert.That(dto2, Is.EqualTo(dto));
        }
    }
}