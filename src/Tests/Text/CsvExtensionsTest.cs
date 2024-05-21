using Urdep.Extensions.Text;

namespace Tests.Text;

/// <summary>
/// Tests for CSV extension methods.
/// </summary>
public class CsvExtensionsTest
{
    /// <summary>
    /// A basic test of CSV extension methods.
    /// </summary>
    [Test]
    public void TestBasic1()
    {
        var keys = new string[] { "A", "B", "C" };
        var recordsData = new string[][] { ["12", "123", "1234"], };

        var columns = keys.ToDictionary(k => k, k => k);
        var records = recordsData.Select(record => MakeDictionary(record, keys));

        var csv = new CsvColumnAligner(columns, records, PadSide.Right).AlignColumns();
        var actualColumns = csv.Columns;
        var actualRecords = csv.Records.ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(actualColumns["A"], Is.EqualTo("A  "));
            Assert.That(actualRecords[0]["A"], Is.EqualTo("12 "));

            Assert.That(actualColumns["B"], Is.EqualTo("B   "));
            Assert.That(actualRecords[0]["B"], Is.EqualTo("123 "));

            Assert.That(actualColumns["C"], Is.EqualTo("C    "));
            Assert.That(actualRecords[0]["C"], Is.EqualTo("1234 "));
        });
    }

    private static Dictionary<string, string> MakeDictionary(string[] record, string[] keys)
    {
        var dict = new Dictionary<string, string>();
        for (int i = 0; i < keys.Length; i++)
        {
            dict[keys[i]] = record[i];
        }
        return dict;
    }
}
